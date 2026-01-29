using Course.Assessment.Payment.Domain.Outbox;
using Course.Assessment.Payment.Domain.Payment;
using Microsoft.EntityFrameworkCore;

namespace Course.Assessment.Payment.Application.Abstractions.Data;
public interface IApplicationDbContext
{
    public DbSet<PaymentEntity> Payments { get; }
    public DbSet<PaymentProvisionEntity> PaymentProvisions { get; }
    public DbSet<OutboxMessageEntity> OutboxMessages { get; }
}
