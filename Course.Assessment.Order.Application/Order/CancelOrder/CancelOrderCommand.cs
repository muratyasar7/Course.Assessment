
using Course.Assessment.Order.Application.Abstractions.Messaging;

namespace Course.Assessment.Order.Application.Order.CancelOrder
{
    public sealed record CancelOrderCommand(string IdempotencyKey, Guid OrderId) : ICommand<Guid>;
}
