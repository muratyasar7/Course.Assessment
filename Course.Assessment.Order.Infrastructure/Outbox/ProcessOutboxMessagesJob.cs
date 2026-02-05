using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Domain.Outbox;
using Course.Assessment.Order.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Analytics.Infrastructure.Outbox;
using Quartz;
using Shared.Contracts.Events;
using Shared.Contracts.Options;
using Shared.Contracts.Queue.Publisher;

[DisallowConcurrentExecution]
internal sealed class ProcessQueueMessagesJob : IJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMessagePublisher _messageBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly OutboxOptions _outboxOptions;
    private readonly ILogger<ProcessQueueMessagesJob> _logger;

    public ProcessQueueMessagesJob(
        ApplicationDbContext dbContext,
        IMessagePublisher messageBus,
        IDateTimeProvider dateTimeProvider,
        IOptions<OutboxOptions> outboxOptions,
        ILogger<ProcessQueueMessagesJob> logger)
    {
        _dbContext = dbContext;
        _messageBus = messageBus;
        _dateTimeProvider = dateTimeProvider;
        _outboxOptions = outboxOptions.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            context.CancellationToken);

        var outboxMessages = await _dbContext.OutboxMessages
        .FromSqlRaw(@"
            SELECT *
            FROM outbox_messages
            WHERE processed_on_utc IS NULL
                AND status = {0}
                ORDER BY occurred_on_utc
                FOR UPDATE SKIP LOCKED
                LIMIT {1}
            ",
            OutboxMessageStatus.Pending,
            _outboxOptions.BatchSize
         )
        .ToListAsync(context.CancellationToken);

        foreach (var outboxMessage in outboxMessages)
        {
            Exception exception = null;
            try
            {
                var eventType = Type.GetType(outboxMessage.Type, throwOnError: true)!;

                var integrationEvent =
                    (IIntegrationEvent)JsonSerializer.Deserialize(
                        outboxMessage.Content,
                        eventType)!;

                await _messageBus.PublishAsync(
                    integrationEvent,
                    new MessagePublishOptions
                    {
                        Topic = eventType.Name.Replace("Event", "Topic"),
                        Key = outboxMessage.Id.ToString(),
                        Headers = new Dictionary<string, string>
                        {
                            ["event_id"] = outboxMessage.Id.ToString(),
                            ["execute_at"] = outboxMessage.OccurredOnUtc.AddMinutes(15).ToString("O"),
                            ["event-type"] = eventType.AssemblyQualifiedName!
                        }
                    },
                    context.CancellationToken);

                outboxMessage.ProcessedOnUtc = _dateTimeProvider.UtcNow;
                outboxMessage.Status = OutboxMessageStatus.Puslished;
                outboxMessage.Error = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception while processing outbox message {MessageId}",
                    outboxMessage.Id);
               exception = ex;
            }
            outboxMessage.ProcessedOnUtc = _dateTimeProvider.UtcNow;
            outboxMessage.Error = exception?.ToString();
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        await transaction.CommitAsync(context.CancellationToken);
    }
}
