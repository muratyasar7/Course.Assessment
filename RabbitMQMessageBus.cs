using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Course.Assessment.Order.Application.Abstractions.Queue;
using Course.Assessment.Order.Domain.Options;
using Polly.Retry;
using RabbitMQ.Client;

namespace Course.Assessment.Order.Infrastructure.Queue
{
    public sealed class RabbitMqMessageBus : IMessageBus
    {
        private readonly IChannel _channel;
        private readonly AsyncRetryPolicy _retryPolicy;


        public RabbitMqMessageBus(IChannel channel)
        {
            _channel = channel;
            _retryPolicy = MessageBusRetryPolicies.Create();
        }

        public async Task PublishAsync<T>(
            T message,
            MessagePublishOptions options,
            CancellationToken cancellationToken = default)
            where T : IIntegrationEvent
        {
            
            await _retryPolicy.ExecuteAsync(async ct =>
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                // IChannel does not define CreateBasicPropertiesAsync.
                // Try to obtain basic properties from the underlying RabbitMQ model implementation.
                var model = _channel as IModel;
                if (model is null)
                {
                    throw new InvalidOperationException("The configured channel does not expose CreateBasicProperties. Ensure the IChannel implementation wraps RabbitMQ.Client.IModel or add a CreateBasicPropertiesAsync method to IChannel.");
                }

                var properties = model.CreateBasicProperties();

                properties.MessageId = message.EventId.ToString();
                properties.Type = message.EventType;
                properties.Timestamp = new AmqpTimestamp(
                    new DateTimeOffset(message.OccurredAt).ToUnixTimeSeconds());

                if (options.Headers != null)
                {
                    properties.Headers = options.Headers
                        .ToDictionary(k => k.Key, v => (object)v.Value);
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