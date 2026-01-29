using Course.Assessment.Order.Domain.Abstractions;

namespace Course.Assessment.Order.Domain.Order.Events;

public sealed record OrderCreatedDomainEvent(Guid OrderId) : IDomainEvent;
