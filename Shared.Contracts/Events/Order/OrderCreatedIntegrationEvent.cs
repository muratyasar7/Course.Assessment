using Shared.Contracts.Events;

namespace Shared.Contracts.QueueMessageEventModels.v1.Order
{
    public sealed record OrderCreatedIntegrationEvent(Guid OrderId, Guid EventId, string EventType, DateTime OccurredOnUtc) : IIntegrationEvent
    {
    }
}
