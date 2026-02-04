using Shared.Contracts.Events;

namespace Shared.Contracts.Queue.Consumer
{
    public interface IRedisStreamConsumer<T> : IMessageConsumer<T>
    where T : IIntegrationEvent;
}
