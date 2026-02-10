namespace Course.Assessment.Order.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public OutboxMessage(Guid id, DateTimeOffset occurredOnUtc, string type, string content)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
        Content = content;
        Type = type;
    }

    public Guid Id { get; init; }

    public DateTimeOffset OccurredOnUtc { get; init; }

    public string Type { get; init; }

    public string Content { get; init; }

    public DateTimeOffset? ProcessedOnUtc { get; init; }

    public string? Error { get; init; }
}
