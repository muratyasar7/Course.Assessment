
using RabbitMQ.Client;

namespace Shared.Contracts.Queue
{
    public interface IBusService
    {
        Task<IChannel> CreateChannel();
    }
}
