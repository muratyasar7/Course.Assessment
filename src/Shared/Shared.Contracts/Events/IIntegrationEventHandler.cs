using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.Events
{
    public interface IIntegrationEventHandler<in T>
    where T : IIntegrationEvent
    {
        Task HandleAsync(T @event, CancellationToken cancellationToken);
    }
}
