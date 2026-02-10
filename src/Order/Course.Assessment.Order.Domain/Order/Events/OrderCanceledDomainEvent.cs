using Course.Assessment.Order.Domain.Abstractions;

namespace Course.Assessment.Order.Domain.Order.Events;

public sealed record OrderCanceledDomainEvent(Guid OrderId) : IDomainEvent;
