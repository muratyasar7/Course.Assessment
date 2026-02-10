

using Course.Assessment.Payment.Application.Abstractions.Messaging;
using Course.Assessment.Payment.Application.Clock;
using Course.Assessment.Payment.Application.Payment.GetOrderPaymentDetail;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Payment;

namespace Course.Assessment.Order.Application.Order.CreateOrder
{
    internal sealed class GetOrderPaymentDetailQueryHandler : IQueryHandler<GetOrderPaymentDetailQuery, OrderDetailRepsonse>
    {
        private readonly IPaymentRepository _paymentRepository;
        public GetOrderPaymentDetailQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<OrderDetailRepsonse>> Handle(GetOrderPaymentDetailQuery command, CancellationToken cancellationToken)
        {
            var response = await _paymentRepository.GetOrderPaymentDetailAsync(x => x.OrderId == command.OrderId);
            if (response == null)
                return Result.Failure<OrderDetailRepsonse>(Error.NullValue);

            return Result.Success(new OrderDetailRepsonse(response!.PaymentReference, response!.Status, response.Status.ToString()));
        }
    }
}
