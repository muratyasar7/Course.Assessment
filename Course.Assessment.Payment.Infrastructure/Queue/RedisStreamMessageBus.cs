using System.Text.Json;
using Course.Assessment.Payment.Application.Abstractions.Queue;
using Course.Assessment.Payment.Domain.Options;
using Polly.Retry;
using Shared.Contracts.Events;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure.Queue
{
    public sealed class RedisStreamMessageBus : IMessageBus
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly AsyncRetryPolicy _retryPolicy;

        public RedisStreamMessageBus(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _retryPolicy = MessageBusRetryPolicies.Create();
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
