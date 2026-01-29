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
            return builder;
        }

        public static async Task<IResult> CreateOrder(CreateOrderRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new CreateOrderCommand(request);
            var result = await sender.Send(command);
            return Results.Created();
        }
        public static async Task<IResult> CancelOrder([FromHeader] string idempotencyKey, [FromRoute] string referenceId, ISender sender, CancellationToken cancellationToken)
        {
            var command = new CancelOrderCommand(idempotencyKey, referenceId);
            var result = await sender.Send(command);
            return Results.Created();
        }
    }
}
