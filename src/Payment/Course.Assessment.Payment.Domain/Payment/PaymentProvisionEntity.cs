using Course.Assessment.Payment.Domain.Abstractions;

namespace Course.Assessment.Payment.Domain.Payment
{
    public sealed class PaymentProvisionEntity : Entity
    {
        public Guid PaymentId { get; private set; }

        public string Provider { get; private set; } = default!;
        public string ProvisionReference { get; private set; } = default!;
        public decimal Amount { get; private set; }

        public ProvisionStatus Status { get; private set; }

        public DateTimeOffset RequestedAt { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }

        private PaymentProvisionEntity() { }
    
        public PaymentProvisionEntity(
            Guid paymentId,
            string provider,
            decimal amount)
        {
            Id = Guid.NewGuid();
            PaymentId = paymentId;
            Provider = provider;
            Amount = amount;

            ProvisionReference = $"PRV-{Guid.NewGuid():N}";
            Status = ProvisionStatus.Requested;
            RequestedAt = DateTimeOffset.UtcNow;
        }

        public void Authorize()
        {
            Status = ProvisionStatus.Authorized;
            CompletedAt = DateTimeOffset.UtcNow;
        }

        public void Capture()
        {
            if (Status != ProvisionStatus.Authorized)
                throw new InvalidOperationException("Provision not authorized.");

            Status = ProvisionStatus.Captured;
            CompletedAt = DateTimeOffset.UtcNow;
        }

        public void Cancel()
        {
            Status = ProvisionStatus.Cancelled;
            CompletedAt = DateTimeOffset.UtcNow;
        }

        public void Fail()
        {
            Status = ProvisionStatus.Failed;
            CompletedAt = DateTimeOffset.UtcNow;
        }
    }

}
