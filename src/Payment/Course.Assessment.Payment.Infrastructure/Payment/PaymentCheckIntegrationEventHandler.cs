using Course.Assessment.Payment.Application.Clock;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Payment;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Payment;

namespace Course.Assessment.Payment.Infrastructure.Payment
{
    public sealed class PaymentCheckIntegrationEventHandler
     : IIntegrationEventHandler<PaymentCheckIntegrationEvent>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentCheckIntegrationEventHandler> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;

        public PaymentCheckIntegrationEventHandler(
            IPaymentRepository paymentRepository,
            ILogger<PaymentCheckIntegrationEventHandler> logger,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task HandleAsync(
            PaymentCheckIntegrationEvent @event,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Handling PaymentCapturedIntegrationEvent. EventId={EventId}, PaymentId={PaymentId}",
                @event.EventId,
                @event.PaymentId);

            var payment = await _paymentRepository
                .GetByIdAsync(@event.PaymentId, ct);

            if (payment?.Status == PaymentStatus.Cancelled)
            {
                _logger.LogWarning(
                   "Payment canceled before {PaymentId}",
                   @event.PaymentId);
                return;
            }
            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment not found {PaymentId}",
                    @event.PaymentId);
                return;
            }

            payment.MarkCaptured(_dateTimeProvider.UtcNow);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Payment captured. PaymentId={PaymentId}",
                @event.PaymentId);
        }
    }


}
