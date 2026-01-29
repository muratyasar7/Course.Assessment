namespace Course.Assessment.Payment.Domain.Payment
{
    public enum ProvisionStatus
    {
        Requested = 1,
        Authorized = 2,
        Captured = 3,
        Cancelled = 4,
        Failed = 5
    }
}
