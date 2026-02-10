

using Course.Assessment.Payment.Application.Abstractions.Messaging;
using Course.Assessment.Payment.Application.Payment.GetOrderPaymentDetail;

namespace Course.Assessment.Order.Application.Order.CreateOrder
{
    public sealed record GetOrderPaymentDetailQuery(Guid OrderId) : IQuery<OrderDetailRepsonse>;
}
