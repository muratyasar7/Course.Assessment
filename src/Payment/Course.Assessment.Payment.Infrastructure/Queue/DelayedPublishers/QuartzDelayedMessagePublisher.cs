using System.Text.Json;
using Quartz;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Publisher;
using Course.Assessment.Payment.Infrastructure.Queue.Consumers.Jobs;

namespace Course.Assessment.Payment.Infrastructure.Queue.DelayedPublishers
{
    public class QuartzDelayedMessagePublisher(ISchedulerFactory schedulerFactory) : IDelayedMessagePublisher 
    {
        public async Task PublishAsync<T>(T message, DateTimeOffset executedAtUtc, CancellationToken ct = default) where T : IIntegrationEvent
        {
            await PublishAtAsync(message, executedAtUtc, ct);
        }

        private async Task PublishAtAsync<T>(T message, DateTimeOffset scheduleTime, CancellationToken ct = default) where T : IIntegrationEvent
        {
            var scheduler = await schedulerFactory.GetScheduler(ct);

            var jobData = new JobDataMap();
            jobData.Put("payload", JsonSerializer.Serialize(message));
            jobData.Put("type", typeof(T).AssemblyQualifiedName);

            var guid = Guid.NewGuid();
            var job = JobBuilder.Create<QuartzDelayedJob<T>>()
                .WithIdentity($"job-{typeof(T).Name}-{guid}")
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"trig-{typeof(T).Name}-{guid}")
                .StartAt(scheduleTime) 
                .Build();
            if (!scheduler.IsStarted)
            {
                await scheduler.Start(ct);
            }
            await scheduler.ScheduleJob(job, trigger, ct);
        }
    }
}
