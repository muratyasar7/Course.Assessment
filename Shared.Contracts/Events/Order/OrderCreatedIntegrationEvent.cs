using Shared.Contracts.Events;

namespace Shared.Contracts.QueueMessageEventModels.v1.Order
{
    public sealed record OrderCreatedIntegrationEvent(Guid OrderId, decimal Amount, string Currency, Guid EventId, string EventType, DateTime OccurredOnUtc) : IIntegrationEvent
    {
    }
}
