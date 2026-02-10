using System;

namespace Course.Assessment.Order.Infrastructure.Outbox;

public sealed class OutboxMessage(
    Guid id,
    DateTimeOffset occurredOnUtc,
    string type,
    string content)
{
    public Guid Id { get; init; } = id;
    public DateTimeOffset OccurredOnUtc { get; init; } = occurredOnUtc;
    public string Type { get; init; } = type;
    public string Content { get; init; } = content;

    public DateTimeOffset? ProcessedOnUtc { get; init; }
    public string Error { get; init; }
}

