using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql.Internal;
using Polly.Retry;
using Shared.Contracts.Events;
using Shared.Contracts.Options;
using Shared.Contracts.Queue.Policies;
using Shared.Contracts.Queue.Publisher;
using StackExchange.Redis;

namespace Course.Assessment.Order.Infrastructure.Queue.Publisher
{
    public sealed class RedisStreamMessagePublisher : IMessagePublisher
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly JsonSerializerOptions _serializerOptions;

        public RedisStreamMessagePublisher(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _retryPolicy = PublisherRetryPolicies.Create();
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
                    options.Destination,
                    result);
            });

        }
    }

}
