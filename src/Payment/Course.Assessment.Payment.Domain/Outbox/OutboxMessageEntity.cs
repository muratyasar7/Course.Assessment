namespace Course.Assessment.Payment.Domain.Outbox
{
    public sealed class OutboxMessageEntity
    {
        private OutboxMessageEntity(DateTimeOffset occurredOnUtc, string type, string content)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = occurredOnUtc;
            Content = content;
            Type = type;
        }
        private OutboxMessageEntity()
        {

        }

        public static OutboxMessageEntity Create(DateTimeOffset occurredOnUtc, string type, string content)
        {
            return new OutboxMessageEntity(occurredOnUtc, type, content);
        }


        public Guid Id { get; init; }


        public DateTimeOffset OccurredOnUtc { get; init; }

        public string Type { get; init; } = "";

        public string Content { get; init; } = "";

        public DateTimeOffset? ProcessedOnUtc { get; set; }

        public OutboxMessageStatus Status { get; set; }

        public string? Error { get; set; }
    }

}
