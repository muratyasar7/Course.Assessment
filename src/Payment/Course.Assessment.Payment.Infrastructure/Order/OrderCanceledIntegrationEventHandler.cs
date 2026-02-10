using Course.Assessment.Payment.Application.Clock;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Payment;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Order;

namespace Course.Assessment.Payment.Infrastructure.Order
{
    public sealed class OrderCanceledIntegrationEventHandler
    : IIntegrationEventHandler<OrderCanceledIntegrationEvent>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;

        public OrderCanceledIntegrationEventHandler(ILogger<OrderCreatedIntegrationEventHandler> logger, IUnitOfWork unitOfWork, IPaymentRepository paymentRepository, IDateTimeProvider dateTimeProvider)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _paymentRepository = paymentRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task HandleAsync(
            OrderCanceledIntegrationEvent @event,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Handling OrderCreatedIntegrationEvent. EventId={EventId}, OrderId={OrderId}",
                @event.EventId,
                @event.OrderId);

            var payment = await _paymentRepository.GetPaymentByOrderId(@event.OrderId, ct);
            if (payment?.Status != PaymentStatus.Initiated)
            {
                _logger.LogWarning(
                   "Payment cannot be canceled {OrderId}",
                   @event.OrderId);

                return;
            }
            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment already exists for OrderId={OrderId}. Skipping.",
                    @event.OrderId);

                return;
            }
            payment!.Cancel(_dateTimeProvider.UtcNow);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Payment canceled. PaymentId={PaymentId}, OrderId={OrderId}",
                payment.Id,
                @event.OrderId);
        }
    }
}
