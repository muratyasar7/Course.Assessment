using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Consumer;
using Shared.Contracts.Queue.Idempotency;

public sealed class KafkaConsumer<TEvent>
    : IMessageConsumer<TEvent>, IDisposable
    where TEvent : IIntegrationEvent
{
    private readonly IConfiguration _configuration;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IMemoryCache _memoryCache;
    private IConsumer<string, string>? _consumer;
    private readonly JsonSerializerOptions _serializerOptions;

    public KafkaConsumer(
        IConfiguration configuration,
        IIdempotencyStore idempotencyStore,
        IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _idempotencyStore = idempotencyStore;
        _memoryCache = memoryCache;

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task ConsumeAsync(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration.GetConnectionString("Kafka"),
            GroupId = "paymentApi",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        var topicName = typeof(TEvent).Name.Replace("Event", "Topic");
        _consumer.Subscribe(topicName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromSeconds(10));
                if (result?.Message?.Value == null)
                    continue;

                var eventTypeHeader = result.Message.Headers?.GetLastBytes("event-type");
                if (eventTypeHeader is null)
                    throw new InvalidOperationException("event-type header missing");

                var eventTypeName = Encoding.UTF8.GetString(eventTypeHeader);
                var eventType = Type.GetType(eventTypeName, throwOnError: true)!;
                var message = JsonSerializer.Deserialize(result.Message.Value, eventType, _serializerOptions)!;
                var evt = (TEvent)message;

                if (_memoryCache.TryGetValue(evt.EventId, out _))
                {
                    _consumer.Commit(result);
                    continue;
                }
                   

                if (await _idempotencyStore.ExistsAsync(evt.EventId))
                {
                    _consumer.Commit(result);
                    continue;
                }
                    

                await handler(evt, cancellationToken);

                await _idempotencyStore.MarkProcessedAsync(evt.EventId);

                _memoryCache.Set(evt.EventId, true, TimeSpan.FromMinutes(10));

                _consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break; // graceful shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // log + DLQ mantığı
            }
        }
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}