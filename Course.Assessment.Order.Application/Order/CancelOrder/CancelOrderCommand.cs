
using Course.Assessment.Order.Application.Abstractions.Messaging;

namespace Course.Assessment.Order.Application.Order.CancelOrder
{
    public sealed record CancelOrderCommand(Guid OrderId) : ICommand<Guid>;
}
