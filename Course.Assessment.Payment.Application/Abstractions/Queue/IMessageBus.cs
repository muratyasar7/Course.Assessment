using Course.Assessment.Payment.Domain.Options;

namespace Course.Assessment.Payment.Application.Abstractions.Queue
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(
            T message,
            MessagePublishOptions options,
            CancellationToken cancellationToken = default)
            where T : IIntegrationEvent;
    }
}
