using Shared.Contracts.Events;

namespace Shared.Contracts
{
    public interface IMessageConsumer<TEvent> where TEvent : IIntegrationEvent
    {
        Task StartAsync(
            Func<TEvent, CancellationToken, Task> handler,
            CancellationToken cancellationToken);
    }
}
