using Course.Assessment.Order.Application.Abstractions.Messaging;
using Course.Assessment.Order.Domain.Shared;
using Course.Assessment.Payment.Domain.Shared;

namespace Course.Assessment.Order.Application.Order.CreateOrder
{
    public sealed record CreateOrderRequest(Guid CustomerId, Money Amount, Address Address, DateTime CreatedAt);
    public sealed record CreateOrderCommand(
        CreateOrderRequest Request) : ICommand<Guid>;
}
