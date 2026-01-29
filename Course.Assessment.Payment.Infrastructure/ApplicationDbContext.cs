using System.Data;
using System.Text.Json;
using Course.Assessment.Payment.Application.Exceptions;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Application.Abstractions.Data;
using Course.Assessment.Payment.Application.Clock;
using Microsoft.EntityFrameworkCore;
using Course.Assessment.Payment.Domain.Payment;
using Course.Assessment.Payment.Domain.Outbox;

namespace Course.Assessment.Payment.Infrastructure
{
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork, IApplicationDbContext
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public DbSet<PaymentEntity> Payments { get; private set; }
        public DbSet<PaymentProvisionEntity> PaymentProvisions { get; private set; }
        public DbSet<OutboxMessageEntity> OutboxMessages { get; private set; }
        public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTimeProvider dateTimeProvider)
        : base(options)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                AddDomainEventsAsOutboxMessages();

                int result = await base.SaveChangesAsync(cancellationToken);

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("Concurrency exception occurred.", ex);
            }
        }


        private void AddDomainEventsAsOutboxMessages()
        {
            var outboxMessages = ChangeTracker
                .Entries<Entity>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    IReadOnlyList<IDomainEvent> domainEvents = entity.GetDomainEvents();

                    entity.ClearDomainEvents();

                    return domainEvents;
                })
                .Select(domainEvent => new OutboxMessageEntity(
                    Guid.NewGuid(),
                    _dateTimeProvider.UtcNow,
                    domainEvent.GetType().Name,
                    JsonSerializer.Serialize(domainEvent)))
                .ToList();

            AddRange(outboxMessages);
        }

    }
}
