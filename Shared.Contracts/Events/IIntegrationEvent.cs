using MediatR;

namespace Shared.Contracts.Events
{
    public interface IIntegrationEvent : INotification
    {
        Guid EventId { get; }
        string EventType { get; }
        DateTimeOffset OccurredOnUtc { get; }
    }
}
