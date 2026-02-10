namespace Course.Assessment.Payment.Domain.Payment
{
    public enum PaymentStatus
    {
        Initiated = 1,
        Provisioned = 2,
        Captured = 3,
        Cancelled = 4,
        Failed = 5
    }
}
