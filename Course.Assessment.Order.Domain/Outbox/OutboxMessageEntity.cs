namespace Course.Assessment.Order.Domain.Outbox
{
    public sealed class OutboxMessageEntity
    {
        public OutboxMessageEntity(DateTime occurredOnUtc, string type, string content)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = occurredOnUtc;
            Content = content;
            Type = type;
        }

        public Guid Id { get; init; }

        public DateTime ExecuteAt { get; set; }

        public DateTime OccurredOnUtc { get; init; }

        public string Type { get; init; }

        public string Content { get; init; }

        public DateTime? ProcessedOnUtc { get; set; }

        public OutboxMessageStatus Status { get; set; }

        public string? Error { get; set; }
    }

}
