using System.Text.Json;
using Course.Assessment.Order.Application.Abstractions.Data;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order.Events;
using Course.Assessment.Order.Domain.Outbox;
using MediatR;
using Shared.Contracts.Events.Order;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Course.Assessment.Order.Application.Order.CreateOrder
{
    internal sealed class OrderCanceledDomainEventHandler : INotificationHandler<OrderCanceledDomainEvent>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCanceledDomainEventHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(OrderCanceledDomainEvent notification, CancellationToken cancellationToken)
        {
            var orderCreatedIntegrationEvent = new OrderCanceledIntegrationEvent(
                            notification.OrderId,
                            Guid.NewGuid(),
                            nameof(OrderCanceledIntegrationEvent),
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
