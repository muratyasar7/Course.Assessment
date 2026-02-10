namespace Course.Assessment.Payment.Domain.Payment
{
    public interface IPaymentProvisionRepository
    {
        Task<PaymentProvisionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Add(PaymentProvisionEntity order);
    }
}
