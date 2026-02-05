using Shared.Contracts.Events;
using Shared.Contracts.Options;

namespace Shared.Contracts.Queue.Publisher
{
    public interface IMessagePublisher
    {
        Task PublishAsync<TEvent>(
            TEvent message,
            MessagePublishOptions options,
            CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent;
    }
}
