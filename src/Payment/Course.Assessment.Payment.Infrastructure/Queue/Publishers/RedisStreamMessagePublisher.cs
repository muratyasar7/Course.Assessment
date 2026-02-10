using System.Text.Json;
using Polly.Retry;
using Shared.Contracts.Events;
using Shared.Contracts.Options;
using Shared.Contracts.Queue.Policies;
using Shared.Contracts.Queue.Publisher;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure.Queue.Publishers
{
    public sealed class RedisStreamMessagePublisher : IMessagePublisher
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly AsyncRetryPolicy _retryPolicy;

        public RedisStreamMessagePublisher(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _retryPolicy = PublisherRetryPolicies.Create();
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

                var values = new NameValueEntry[]
                {
                    new("eventId", message.EventId.ToString()),
                    new("eventType", message.EventType),
                    new("payload", JsonSerializer.Serialize(message))
                };
                await db.StreamAddAsync(
                    options.Destination,
                    values);
            });

        }
    }

}
