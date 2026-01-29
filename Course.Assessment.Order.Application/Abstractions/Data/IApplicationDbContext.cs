using Course.Assessment.Order.Domain.Order;
using Course.Assessment.Order.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Course.Assessment.Order.Application.Abstractions.Data;
public interface IApplicationDbContext
{
    public DbSet<OrderEntity> Orders { get; }
    public DbSet<OutboxMessageEntity> OutboxMessages { get; }
}
