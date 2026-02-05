using System.Text;
using System.Text.Json;
using Course.Assessment.Payment.Application.Abstractions.Queue;
using Course.Assessment.Payment.Domain.Options;
using Polly.Retry;
using RabbitMQ.Client;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Policies;

namespace Course.Assessment.Payment.Infrastructure.Queue.Publishers
{
    public sealed class RabbitMqMessageBus : IMessagePublisher
    {
        private readonly IChannel _channel;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly JsonSerializerOptions _serializerOptions;



        public RabbitMqMessageBus(IChannel channel)
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
                };
                properties.MessageId = message.EventId.ToString();
                properties.Type = message.EventType;
                properties.Timestamp = new AmqpTimestamp(
                    new DateTimeOffset(message.OccurredOnUtc).ToUnixTimeSeconds());

                if (options.Headers != null)
                {
                    properties.Headers = options.Headers
                        .ToDictionary(k => k.Key, v => (object?)v.Value);
                }

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