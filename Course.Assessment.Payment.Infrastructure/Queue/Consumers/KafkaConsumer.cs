using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Consumer;

public sealed class KafkaConsumer<TEvent>
    : IMessageConsumer<TEvent>, IDisposable
    where TEvent : IIntegrationEvent
{
    private readonly IConfiguration _configuration;
    private IConsumer<string, string>? _consumer;
    private readonly JsonSerializerOptions _serializerOptions;


    public KafkaConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
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
        var topicName = typeof(TEvent).Name.Replace("Event","Topic");
        _consumer.Subscribe(topicName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromSeconds(10));

                if (result?.Message?.Value == null)
                    continue;
                var eventTypeHeader = result.Message.Headers?
                    .GetLastBytes("event-type");

                if (eventTypeHeader is null)
                    throw new InvalidOperationException("event-type header missing");

                var eventTypeName = Encoding.UTF8.GetString(eventTypeHeader);
                var eventType = Type.GetType(eventTypeName, throwOnError: true)!;
                var message = JsonSerializer.Deserialize(
                       result.Message.Value,
                       eventType,_serializerOptions)!;

                await handler((TEvent)message, cancellationToken);

                _consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break; // graceful shutdown
            }
            catch (Exception ex)
            {
                // log + DLQ
            }
        }
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}
