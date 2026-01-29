namespace Course.Assessment.Payment.Domain.Payment
{
    public interface IPaymentRepository
    {
        Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Add(PaymentEntity order);
    }
}
