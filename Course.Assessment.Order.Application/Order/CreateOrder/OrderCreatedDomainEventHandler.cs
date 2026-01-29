using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Course.Assessment.Order.Application.Abstractions.Data;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Domain.Abstractions;
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
            _dbContext.OutboxMessages.Add(
                new OutboxMessageEntity(
                    Guid.NewGuid(),
                    _dateTimeProvider.UtcNow,
                    nameof(OrderCreatedIntegrationEvent),
                    JsonSerializer.Serialize(
                        new OrderCreatedIntegrationEvent(
                            notification.OrderId,
                            Guid.NewGuid(),
                            nameof(OrderCreatedIntegrationEvent),
                            _dateTimeProvider.UtcNow)
                        )
                    )
                );
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
