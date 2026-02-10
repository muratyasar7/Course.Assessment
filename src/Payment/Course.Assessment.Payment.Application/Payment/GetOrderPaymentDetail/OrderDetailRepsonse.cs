using System;
using System.Collections.Generic;
using System.Text;
using Course.Assessment.Payment.Domain.Payment;

namespace Course.Assessment.Payment.Application.Payment.GetOrderPaymentDetail
{
    internal sealed record OrderDetailRepsonse(string PaymentReference, PaymentStatus PaymentStatus, string PaymentStatustText);
}
