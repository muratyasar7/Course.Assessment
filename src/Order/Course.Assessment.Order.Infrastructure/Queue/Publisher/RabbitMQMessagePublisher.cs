using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly.Retry;
using RabbitMQ.Client;
using Shared.Contracts.Events;
using Shared.Contracts.Options;
using Shared.Contracts.Queue.Policies;
using Shared.Contracts.Queue.Publisher;
using StackExchange.Redis;

namespace Course.Assessment.Order.Infrastructure.Queue.Publisher
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
                var exchangeName = message.GetType().Name;
                await _channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    cancellationToken: cancellationToken);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
                    message,
                    message.GetType(),
                    _serializerOptions));
                var properties = new BasicProperties
                {
                    Persistent = true,
                    MessageId = message.EventId.ToString(),
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                    DeliveryMode = DeliveryModes.Persistent,
                    Expiration = "60000"
                };
                properties.MessageId = message.EventId.ToString();
                properties.Type = message.EventType;
                properties.Timestamp = new AmqpTimestamp(
                    message.OccurredOnUtc.ToUnixTimeSeconds());

                if (options.Headers != null)
                {
                    properties.Headers = options.Headers
                        .ToDictionary(k => k.Key, v => (object)v.Value);
                }

                await _channel.BasicPublishAsync(
                     exchange: exchangeName,
                     routingKey: string.Empty,
                     mandatory: false,
                     basicProperties: properties,
                     body: body,
                     cancellationToken: cancellationToken);
            }, cancellationToken);


        }
    }
}