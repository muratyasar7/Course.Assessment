using Course.Assessment.Payment.Domain.Options;
using Shared.Contracts.Events;

namespace Course.Assessment.Payment.Application.Abstractions.Queue
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(
            T message,
            MessagePublishOptions options,
            CancellationToken cancellationToken = default)
            where T : IIntegrationEvent;
    }
}
