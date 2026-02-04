
using RabbitMQ.Client;

namespace Shared.Contracts.Queue.Publisher
{
    public interface IMessagePublisher
    {
        Task<IChannel> CreateChannel();
    }
}
