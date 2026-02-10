using Course.Assessment.Order.Application.Order.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Course.Assessment.Order.API.Endpoints
{
    public static class PaymentEndpoints
    {
        
        public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapGet("/payment/{orderId}", GetOrderPaymentDetail);
            return builder;
        }
        public static async Task<IResult> GetOrderPaymentDetail([FromRoute]Guid orderId, ISender sender, CancellationToken cancellationToken)
        { 
            
            var detailQuery = new GetOrderPaymentDetailQuery(orderId);
            var result = await sender.Send(detailQuery);
            return Results.Ok(result);
        }
    }
}
