namespace Shared.Contracts.Events.Order
{
    public sealed record OrderCanceledIntegrationEvent(Guid OrderId, Guid EventId, string EventType, DateTime OccurredOnUtc) : IIntegrationEvent;
}
