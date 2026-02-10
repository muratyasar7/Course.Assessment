namespace Course.Assessment.Order.Domain.Outbox
{
    public enum OutboxMessageStatus
    {
        Pending,
        Puslished,
        Canceled
    }
}
