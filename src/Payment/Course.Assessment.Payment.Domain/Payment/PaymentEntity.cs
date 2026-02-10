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

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private readonly List<PaymentProvisionEntity> _provisions = [];
        public IReadOnlyCollection<PaymentProvisionEntity> Provisions => _provisions;

        private PaymentEntity() { }

        public PaymentEntity(
            Guid orderId,
            decimal amount,
            string currency,
            DateTimeOffset createdAt)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            Amount = amount;
            Currency = currency;

            PaymentReference = $"PAY-{Guid.NewGuid():N}";
            Status = PaymentStatus.Initiated;
            CreatedAt = createdAt;
        }

        public static PaymentEntity Create(Guid orderId, decimal amount, string currency, DateTimeOffset createdAt)
        {
            return new PaymentEntity(orderId, amount, currency, createdAt);
        }

        public PaymentProvisionEntity CreateProvision(string provider)
        {
            if (Status != PaymentStatus.Initiated)
                throw new InvalidOperationException("Provision cannot be created.");

            var provision = new PaymentProvisionEntity(Id, provider, Amount);
            _provisions.Add(provision);

            return provision;
        }

        public void MarkProvisioned(DateTimeOffset dateTimeOffset)
        {
            Status = PaymentStatus.Provisioned;
            UpdatedAt = dateTimeOffset;
        }

        public void MarkCaptured(DateTimeOffset dateTimeOffset)
        {
            Status = PaymentStatus.Captured;
            UpdatedAt = dateTimeOffset;
        }

        public void MarkFailed(DateTimeOffset dateTimeOffset)
        {
            Status = PaymentStatus.Failed;
            UpdatedAt = dateTimeOffset;
        }

        public void Cancel(DateTimeOffset dateTimeOffset)
        {
            if (Status == PaymentStatus.Captured)
                throw new InvalidOperationException("Captured payment cannot be cancelled.");

            Status = PaymentStatus.Cancelled;
            UpdatedAt = dateTimeOffset;
        }
    }

}
