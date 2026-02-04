using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Course.Assessment.Order.Application.Abstractions.Queue;
using Course.Assessment.Order.Domain.Options;
using Npgsql.Internal;
using Polly.Retry;
using Shared.Contracts.Events;
using StackExchange.Redis;

namespace Course.Assessment.Order.Infrastructure.Queue.Publisher
{
    public sealed class RedisStreamMessageBus : IMessageBus
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly JsonSerializerOptions _serializerOptions;

        public RedisStreamMessageBus(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _retryPolicy = MessageBusRetryPolicies.Create();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task PublishAsync<T>(
            T message,
            MessagePublishOptions options,
            CancellationToken cancellationToken = default)
            where T : IIntegrationEvent
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var db = _redis.GetDatabase();
                var payload = JsonSerializer.Serialize(
                    message,
                    message.GetType(),
                    _serializerOptions);
                var values = new List<NameValueEntry>
                {
                    new("eventId", message.EventId.ToString()),
                    new("eventType", message.EventType),
                    new("payload", payload)
                };

                foreach (var item in options.Headers)
                {
                    values.Add(new NameValueEntry(item.Key, item.Value));
                }

                var result = values.ToArray();

                await db.StreamAddAsync(
                    options.Topic,
                    result);
            });

        }
    }

}
