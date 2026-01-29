using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Course.Assessment.Order.Application.Abstractions.Data;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Application.Exceptions;
using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order;
using Course.Assessment.Order.Domain.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Course.Assessment.Order.Infrastructure
{
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork, IApplicationDbContext
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPublisher _publisher;
        public DbSet<OrderEntity> Orders { get; private set; }
        public DbSet<OutboxMessageEntity> OutboxMessages { get; private set; }
        public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTimeProvider dateTimeProvider,
        IPublisher publisher)
        : base(options)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _publisher = publisher;
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
                var domainEvents = GetDomainEvents();
                foreach (var domainEvent in domainEvents)
                {
                    await _publisher.Publish(domainEvent,cancellationToken);
                }
                int result = await base.SaveChangesAsync(cancellationToken);

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("Concurrency exception occurred.", ex);
            }
        }


        private List<IDomainEvent> GetDomainEvents()
        {
            return ChangeTracker
                .Entries<Entity>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    IReadOnlyList<IDomainEvent> domainEvents = entity.GetDomainEvents();

                    entity.ClearDomainEvents();

                    return domainEvents;
                }).ToList();

        }

    }
}
