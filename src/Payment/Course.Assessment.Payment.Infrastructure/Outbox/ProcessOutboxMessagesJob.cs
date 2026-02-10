using System.Text.Json;
using Course.Assessment.Payment.Application.Clock;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Outbox;
using Course.Assessment.Payment.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Analytics.Infrastructure.Outbox;
using Quartz;
using Shared.Contracts.Events;

[DisallowConcurrentExecution]
internal sealed class ProcessQueueMessagesJob : IJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly OutboxOptions _outboxOptions;
    private readonly ILogger<ProcessQueueMessagesJob> _logger;

    public ProcessQueueMessagesJob(
        IPublisher publisher,
        IDateTimeProvider dateTimeProvider,
        IOptions<OutboxOptions> outboxOptions,
        ILogger<ProcessQueueMessagesJob> logger,
        ApplicationDbContext dbContext)
    {
        _publisher = publisher;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _outboxOptions = outboxOptions.Value;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var outboxMessages = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null && x.Status == OutboxMessageStatus.Pending)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(_outboxOptions.BatchSize)
            .ToListAsync(context.CancellationToken);

        foreach (var outboxMessage in outboxMessages)
        {
            Exception? exception = null;

            try
            {
                var integrationEvent = JsonSerializer.Deserialize<IIntegrationEvent>(outboxMessage.Content);
                if (integrationEvent != null)
                    await _publisher.Publish(integrationEvent, context.CancellationToken);
                else
                    continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while processing outbox message {MessageId}", outboxMessage.Id);
                exception = ex;
            }

            outboxMessage.ProcessedOnUtc = _dateTimeProvider.UtcNow;
            outboxMessage.Error = exception?.ToString();
        }
        if (outboxMessages.Count > 0)
        {
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await transaction.CommitAsync();
        }
        else
        {
            await transaction.DisposeAsync();
        }
    }
}
