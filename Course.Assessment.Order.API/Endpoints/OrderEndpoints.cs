using Course.Assessment.Order.Application.Order.CancelOrder;
using Course.Assessment.Order.Application.Order.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Course.Assessment.Order.API.Endpoints
{
    public static class OrderEndpoints
    {
        public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("order", CreateOrder);
            builder.MapPatch("order/cancel/{orderId}", CancelOrder);
            return builder;
        }

        public static async Task<IResult> CreateOrder(CreateOrderRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new CreateOrderCommand(request);
            var result = await sender.Send(command);
            return Results.Created();
        }
        public static async Task<IResult> CancelOrder([FromHeader] string idempotencyKey, [FromRoute] Guid orderId, ISender sender, CancellationToken cancellationToken)
        {
            var command = new CancelOrderCommand(idempotencyKey, orderId);
            var result = await sender.Send(command);
            return Results.Created();
        }
    }
}
