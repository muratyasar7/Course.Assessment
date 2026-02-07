namespace Course.Assessment.Order.Domain.Outbox
{
    public sealed class OutboxMessageEntity
    {
        private OutboxMessageEntity(DateTimeOffset occurredOnUtc, string type, string content, DateTimeOffset? executeAt = null)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = occurredOnUtc;
            Content = content;
            Type = type;
            ExecuteAt = executeAt;
        }
        private OutboxMessageEntity()
        {
            
        }

        public static OutboxMessageEntity Create(DateTimeOffset occurredOnUtc, string type, string content, DateTimeOffset? executeAt = null)
        {
            return new OutboxMessageEntity(occurredOnUtc, type, content, executeAt);
        }


        public Guid Id { get; init; }

        public DateTimeOffset? ExecuteAt { get; set; }

        public DateTimeOffset OccurredOnUtc { get; init; }

        public string Type { get; init; } = "";

        public string Content { get; init; } = "";

        public DateTimeOffset? ProcessedOnUtc { get; set; }

        public OutboxMessageStatus Status { get; set; }

        public string? Error { get; set; }
    }

}
