using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.Events.Payment
{
    public sealed record PaymentCheckIntegrationEvent(Guid PaymentId, Guid EventId, string EventType, DateTimeOffset OccurredOnUtc) : IIntegrationEvent
    {

    }
}
