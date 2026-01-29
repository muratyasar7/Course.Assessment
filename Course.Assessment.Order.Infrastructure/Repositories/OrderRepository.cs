using Course.Assessment.Order.Domain.Order;

namespace Course.Assessment.Order.Infrastructure.Repositories;

internal sealed class OrderRepository : Repository<OrderEntity>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {

    }
}
