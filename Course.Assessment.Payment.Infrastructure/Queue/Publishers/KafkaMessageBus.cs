using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Course.Assessment.Payment.Application.Abstractions.Queue;
using Course.Assessment.Payment.Domain.Options;
using Polly.Retry;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Policies;

namespace Course.Assessment.Payment.Infrastructure.Queue.Publishers
{
    public sealed class KafkaMessageBus : IMessagePublisher
    {
        private readonly IProducer<string, string> _producer;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly AsyncRetryPolicy _retryPolicy;


        public KafkaMessageBus(IProducer<string, string> producer)
        {
            _producer = producer;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
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
                var payload = JsonSerializer.Serialize(message, _serializerOptions);

                var kafkaMessage = new Message<string, string>
                {
                    Key = options.Key ?? message.EventId.ToString(),
                    Value = payload,
                    Headers = new Headers()
                };

                if (options.Headers != null)
                {
                    foreach (var header in options.Headers)
                    {
                        kafkaMessage.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
                    }
                }

                await _producer.ProduceAsync(
                    options.Destination,
                    kafkaMessage,
                    ct);
            }, cancellationToken);

        }
    }


}
