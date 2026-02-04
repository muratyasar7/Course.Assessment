using System.Text.Json;
using Course.Assessment.Order.Application.Abstractions.Data;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order;
using Course.Assessment.Order.Domain.Order.Events;
using Course.Assessment.Order.Domain.Outbox;
using MediatR;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Course.Assessment.Order.Application.Order.CreateOrder
{
    internal sealed class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreatedDomainEventHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork, IOrderRepository orderRepository)
        {
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var orderCreatedIntegrationEvent = new OrderCreatedIntegrationEvent(
                            notification.OrderId,
                            notification.Amount,
                            notification.Currency,
                            Guid.NewGuid(),
                            nameof(OrderCreatedIntegrationEvent),
                            _dateTimeProvider.UtcNow);
            _dbContext.OutboxMessages.Add(
                new OutboxMessageEntity(
                    _dateTimeProvider.UtcNow,
                    orderCreatedIntegrationEvent.GetType().AssemblyQualifiedName!,
                    JsonSerializer.Serialize(
                        orderCreatedIntegrationEvent
                    )
                ));
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
