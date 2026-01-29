using System.Text;
using System.Text.Json;
using Course.Assessment.Payment.Application.Abstractions.Queue;
using Course.Assessment.Payment.Domain.Options;
using Polly.Retry;
using RabbitMQ.Client;
using Shared.Contracts.Events;

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
                //var properties = await _channel.CreateBasicProperties(cancellationToken);

                //properties.MessageId = message.EventId.ToString();
                //properties.Type = message.EventType;
                //properties.Timestamp = new AmqpTimestamp(
                //    new DateTimeOffset(message.OccurredAt).ToUnixTimeSeconds());

                //if (options.Headers != null)
                //{
                //    properties.Headers = options.Headers
                //        .ToDictionary(k => k.Key, v => (object)v.Value);
                //}

                await _channel.BasicPublishAsync(
                    exchange: options.Destination,
                    routingKey: "",
                    mandatory: false,
                    //basicProperties: properties,
                    body: body,
                    cancellationToken: ct);
            }, cancellationToken);


        }
    }
}