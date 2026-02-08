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
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreatedDomainEventHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var date = _dateTimeProvider.UtcNow;
            var orderCreatedIntegrationEvent = new OrderCreatedIntegrationEvent(
                            notification.OrderId,
                            notification.Amount,
                            notification.Currency,
                            date.AddMinutes(15),
                            Guid.NewGuid(),
                            nameof(OrderCreatedIntegrationEvent),
                            date);
            _dbContext.OutboxMessages.Add(
                OutboxMessageEntity.Create(
                    date,
                    orderCreatedIntegrationEvent.GetType().AssemblyQualifiedName!,
                    JsonSerializer.Serialize(
                        orderCreatedIntegrationEvent
                    )
                ));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        }
    }
}
