using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Order;
using Shared.Contracts.Events.Payment;
using Shared.Contracts.Queue.Consumer;

namespace Course.Assessment.Payment.Infrastructure.Queue.Consumers.HostedServices
{
    public sealed class PaymentCapturedConsumerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PaymentCapturedConsumerHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumerScope = _scopeFactory.CreateScope();

            var consumer =
                consumerScope.ServiceProvider
                    .GetRequiredService<IMessageConsumer<PaymentCheckIntegrationEvent>>();

            await consumer.ConsumeAsync(async (evt, ct) =>
            {
                using var messageScope = _scopeFactory.CreateScope();

                var handler =
                    messageScope.ServiceProvider
                        .GetRequiredService<
                            IIntegrationEventHandler<PaymentCheckIntegrationEvent>>();

                await handler.HandleAsync(evt, ct);

            }, stoppingToken);
        }
    }
}
