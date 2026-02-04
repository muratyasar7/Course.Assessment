using System;
using System.Collections.Generic;
using System.Text;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Order;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Course.Assessment.Payment.Infrastructure.Order
{
    public sealed class OrderCanceledIntegrationEventHandler
    : IIntegrationEventHandler<OrderCanceledIntegrationEvent>
    {
        public Task HandleAsync(OrderCanceledIntegrationEvent @event, CancellationToken ct)
        {

            return Task.CompletedTask;
        }
    }
}
