using Course.Assessment.Payment.Infrastructure.Order;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Consumer;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Course.Assessment.Payment.Infrastructure.Queue.Consumers.HostedServices
{
    public sealed class OrderCreatedConsumerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderCreatedConsumerHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumerScope = _scopeFactory.CreateScope();

            var consumer =
                consumerScope.ServiceProvider
                    .GetRequiredService<IMessageConsumer<OrderCreatedIntegrationEvent>>();

            await consumer.ConsumeAsync(async (evt, ct) =>
            {
                using var messageScope = _scopeFactory.CreateScope();

                var handler =
                    messageScope.ServiceProvider
                        .GetRequiredService<
                            IIntegrationEventHandler<OrderCreatedIntegrationEvent>>();

                await handler.HandleAsync(evt, ct);

            }, stoppingToken);
        }
    }
}


