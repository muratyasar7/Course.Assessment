using System.Linq.Expressions;

namespace Course.Assessment.Payment.Domain.Payment
{
    public interface IPaymentRepository
    {
        Task<PaymentEntity?> GetOrderPaymentDetailAsync(Expression<Func<PaymentEntity, bool>> filter,CancellationToken cancellationToken= default);
        Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Add(PaymentEntity order);
    }
}
