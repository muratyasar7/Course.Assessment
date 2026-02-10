using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Shared.Contracts.Events;

namespace Course.Assessment.Payment.Infrastructure.Queue.Consumers.Jobs
{
    public class QuartzDelayedJob<T> : IJob where T : IIntegrationEvent
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QuartzDelayedJob<T>> _logger;

        public QuartzDelayedJob(
            IServiceScopeFactory scopeFactory,
            ILogger<QuartzDelayedJob<T>> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("QuartzDelayedJob Execute başladı");

            var payloadJson = context.MergedJobDataMap.GetString("payload");
            var typeName = context.MergedJobDataMap.GetString("type");

            if (payloadJson is null || typeName is null)
                return;

            var eventType = Type.GetType(typeName);
            if (eventType is null)
                return;

            var integrationEvent =
                JsonSerializer.Deserialize(payloadJson, eventType);

            using var scope = _scopeFactory.CreateScope();

            var handlerInterface = typeof(IIntegrationEventHandler<>)
                .MakeGenericType(eventType);

            var handler =
                scope.ServiceProvider.GetRequiredService(handlerInterface);

            var handleMethod = handlerInterface.GetMethod(
                "HandleAsync",
                new[] { eventType, typeof(CancellationToken) });

            if (handleMethod is null)
                throw new InvalidOperationException(
                    $"HandleAsync bulunamadı: {handlerInterface.Name}");

            await (Task)handleMethod!.Invoke(
                handler,
                new[] { integrationEvent!, context.CancellationToken });
        }
    }

}
