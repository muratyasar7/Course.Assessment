using Shared.Contracts.Events;

namespace Shared.Contracts.Queue.Consumer
{
    public interface IMessageConsumer<TEvent> where TEvent : IIntegrationEvent
    {
        Task ConsumeAsync(
            Func<TEvent, CancellationToken, Task> handler,
            CancellationToken cancellationToken);
    }
}
