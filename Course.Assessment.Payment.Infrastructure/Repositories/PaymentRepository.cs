using Course.Assessment.Order.Infrastructure.Repositories;
using Course.Assessment.Payment.Domain.Payment;
using Course.Assessment.Payment.Infrastructure;

namespace Bookify.Infrastructure.Repositories;

internal sealed class PaymentRepository : Repository<PaymentEntity>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {

    }
}
