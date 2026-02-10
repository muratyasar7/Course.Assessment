namespace Course.Assessment.Payment.Domain.Outbox
{
    public enum OutboxMessageStatus
    {
        Pending,
        Puslished,
        Canceled
    }
}
