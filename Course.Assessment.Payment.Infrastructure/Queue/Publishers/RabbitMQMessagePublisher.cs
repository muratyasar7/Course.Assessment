using System.Text;
using System.Text.Json;
using Polly.Retry;
using RabbitMQ.Client;
using Shared.Contracts.Events;
using Shared.Contracts.Options;
using Shared.Contracts.Queue.Policies;
using Shared.Contracts.Queue.Publisher;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure.Queue.Publishers
{
    public sealed class RabbitMQMessagePublisher : IMessagePublisher
    {
        private readonly IChannel _channel;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly JsonSerializerOptions _serializerOptions;



        public RabbitMQMessagePublisher(IChannel channel)
        {
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _channel = channel;
            _retryPolicy = PublisherRetryPolicies.Create();
        }

        public async Task PublishAsync<T>(
            T message,
            MessagePublishOptions options,
            CancellationToken cancellationToken = default)
            where T : IIntegrationEvent
        {

            await _retryPolicy.ExecuteAsync(async ct =>
            {
                var payload = JsonSerializer.Serialize(
                    message,
                    message.GetType(),
                    _serializerOptions);
                var body = Encoding.UTF8.GetBytes(payload);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    MessageId = message.EventId.ToString(),
                    ContentType = "application/json",
                    Headers = new Dictionary<string, object?>()
                };
                properties.MessageId = message.EventId.ToString();
                properties.Type = message.EventType;
                properties.Timestamp = new AmqpTimestamp(
                    message.OccurredOnUtc.ToUnixTimeSeconds());

                if (options.Headers != null)
                {
                    properties.Headers = options.Headers
                        .ToDictionary(k => k.Key, v => (object?)v.Value);
                }
                if (options.Delay.HasValue)
                    properties.Headers["x-delay"] = (int)options.Delay.Value.TotalMilliseconds;


                await _channel.BasicPublishAsync(
                    exchange: options.Destination,
                    routingKey: "",
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: ct);
            }, cancellationToken);


        }
    }
}