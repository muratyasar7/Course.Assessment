using MediatR;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Course.Assessment.Payment.Application.Order
{
    internal class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedIntegrationEvent>
    {
    }
}
