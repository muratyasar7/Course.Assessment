using System.Linq.Expressions;
using Course.Assessment.Order.Infrastructure.Repositories;
using Course.Assessment.Payment.Domain.Payment;
using Course.Assessment.Payment.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories;

internal sealed class PaymentRepository : Repository<PaymentEntity>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {

    }

    public Task<PaymentEntity?> GetOrderPaymentDetailAsync(Expression<Func<PaymentEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return  DbContext.Payments
           .AsNoTracking()
           .FirstOrDefaultAsync(filter, cancellationToken);
    }

    public Task<bool> ExistsByOrderIdAsync(
        Guid orderId,
        CancellationToken ct)
    {
        return DbContext.Payments
            .AsNoTracking()
            .AnyAsync(p => p.OrderId == orderId, ct);
    }
}
