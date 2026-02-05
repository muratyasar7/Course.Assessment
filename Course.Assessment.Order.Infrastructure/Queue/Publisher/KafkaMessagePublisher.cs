using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Polly.Retry;
using Shared.Contracts.Events;
using Shared.Contracts.Options;
using Shared.Contracts.Queue.Policies;
using Shared.Contracts.Queue.Publisher;

namespace Course.Assessment.Order.Infrastructure.Queue.Publisher
{
    public sealed class KafkaMessagePublisher : IMessagePublisher
    {
        private readonly IProducer<string, string> _producer;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly AsyncRetryPolicy _retryPolicy;


        public KafkaMessagePublisher(IProducer<string, string> producer)
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
                var payload = JsonSerializer.Serialize(
                    message,
                    message.GetType(),
                    _serializerOptions);

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
                    options.Topic,
                    kafkaMessage,
                    ct);
            }, cancellationToken);

        }
    }


}
