using Course.Assessment.Payment.Domain.Abstractions;

namespace Course.Assessment.Payment.Domain.Payment
{
    public sealed class PaymentEntity : Entity
    {
        public Guid OrderId { get; private set; }

        public string PaymentReference { get; private set; } = default!;
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = default!;

        public PaymentStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<PaymentProvisionEntity> _provisions = new();
        public IReadOnlyCollection<PaymentProvisionEntity> Provisions => _provisions;

        private PaymentEntity() { }

        public PaymentEntity(
            Guid orderId,
            decimal amount,
            string currency)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            Amount = amount;
            Currency = currency;

            PaymentReference = $"PAY-{Guid.NewGuid():N}";
            Status = PaymentStatus.Initiated;
            CreatedAt = DateTime.UtcNow;
        }

        public static PaymentEntity Create(Guid orderId,decimal amount,string currency)
        {
           return new PaymentEntity(orderId, amount, currency);
        }

        public PaymentProvisionEntity CreateProvision(string provider)
        {
            if (Status != PaymentStatus.Initiated)
                throw new InvalidOperationException("Provision cannot be created.");

            var provision = new PaymentProvisionEntity(Id, provider, Amount);
            _provisions.Add(provision);

            return provision;
        }

        public void MarkProvisioned()
        {
            Status = PaymentStatus.Provisioned;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkCaptured()
        {
            Status = PaymentStatus.Captured;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkFailed()
        {
            Status = PaymentStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == PaymentStatus.Captured)
                throw new InvalidOperationException("Captured payment cannot be cancelled.");

            Status = PaymentStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}
