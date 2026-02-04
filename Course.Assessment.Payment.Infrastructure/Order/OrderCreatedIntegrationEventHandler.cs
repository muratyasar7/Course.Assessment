using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Payment;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Course.Assessment.Payment.Infrastructure.Order
{
    public sealed class OrderCreatedIntegrationEventHandler
     : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;

        public OrderCreatedIntegrationEventHandler(
            IPaymentRepository paymentRepository,
            ILogger<OrderCreatedIntegrationEventHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(
            OrderCreatedIntegrationEvent @event,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Handling OrderCreatedIntegrationEvent. EventId={EventId}, OrderId={OrderId}",
                @event.EventId,
                @event.OrderId);

            var exists = await _paymentRepository
                .ExistsByOrderIdAsync(@event.OrderId, ct);

            if (exists)
            {
                _logger.LogWarning(
                    "Payment already exists for OrderId={OrderId}. Skipping.",
                    @event.OrderId);

                return;
            }

            var payment = PaymentEntity.Create(@event.OrderId,@event.Amount,@event.Currency);
            payment.CreateProvision("GooglePay");
            _paymentRepository.Add(payment);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Payment created. PaymentId={PaymentId}, OrderId={OrderId}",
                payment.Id,
                @event.OrderId);
        }
    }


}
