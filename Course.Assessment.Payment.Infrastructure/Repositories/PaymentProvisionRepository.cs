using Course.Assessment.Order.Infrastructure.Repositories;
using Course.Assessment.Payment.Domain.Payment;

namespace Course.Assessment.Payment.Infrastructure.Repositories;

internal sealed class PaymentProvisionRepository : Repository<PaymentProvisionEntity>, IPaymentProvisionRepository
{
    public PaymentProvisionRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {

    }
}
