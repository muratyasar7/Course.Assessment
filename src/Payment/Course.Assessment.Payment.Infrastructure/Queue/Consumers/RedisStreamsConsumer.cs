using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Course.Assessment.Payment.Domain.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Polly.Retry;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Consumer;
using Shared.Contracts.Queue.Idempotency;
using Shared.Contracts.Queue.Policies;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure.Queue.Consumers;

public sealed class RedisStreamConsumer<TEvent> : IMessageConsumer<TEvent> where TEvent : IIntegrationEvent
{
    private readonly IConnectionMultiplexer _redis;

    private readonly string _streamName;
    private readonly string _groupName;
    private readonly string _consumerName;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IMemoryCache _memoryCache;


    public RedisStreamConsumer(
        IConfiguration configuration,
        IConnectionMultiplexer redis,
        IIdempotencyStore idempotencyStore,
        IMemoryCache memoryCache)
    {
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _redis = redis;
        _streamName = typeof(TEvent).Name.Replace("Event", "Topic");
        _groupName = "PaymentGroup";
        _consumerName = $"{Environment.MachineName}-{Guid.NewGuid()}";
        _retryPolicy = ConsumerRetryPolicies.Create();
        _idempotencyStore = idempotencyStore;
        _memoryCache = memoryCache;
    }

    public async Task ConsumeAsync(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        try
        {
            await db.StreamCreateConsumerGroupAsync(
                _streamName,
                _groupName,
                StreamPosition.NewMessages);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {

        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    _streamName,
                    _groupName,
                    _consumerName,
                    ">",
                    count: 10);

                if (entries.Length == 0)
                {
                    await Task.Delay(500, cancellationToken);
                    continue;
                }

                foreach (var entry in entries)
                {

                    var json = entry.Values
                        .First(x => x.Name == "payload")
                        .Value
                        .ToString();

                    var eventTypeName = entry.Values
                        .First(x => x.Name == "event-type")
                        .Value
                        .ToString();

                    if (eventTypeName is null)
                        throw new InvalidOperationException("event-type header missing");

                    var eventType = Type.GetType(eventTypeName, throwOnError: true)!;
                    var message = JsonSerializer.Deserialize(
                           json,
                           eventType, _serializerOptions)!;
                    var evt = (TEvent)message;

                    if (_memoryCache.TryGetValue(evt.EventId, out _))
                    {
                        await db.StreamAcknowledgeAsync(_streamName, _groupName, entry.Id);
                        continue;
                    }

                    if (await _idempotencyStore.ExistsAsync(evt.EventId))
                    {
                        await db.StreamAcknowledgeAsync(_streamName, _groupName, entry.Id);
                        continue;
                    }

                    await _retryPolicy.ExecuteAsync(async ct =>
                    {
                        await handler(evt, ct);
                    }, cancellationToken);

                    await db.StreamAcknowledgeAsync(_streamName, _groupName, entry.Id);
                }
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception)
            {
                // logging
            }
        }
    }
}
