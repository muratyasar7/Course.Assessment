using Course.Assessment.Order.Domain.Abstractions;
using MediatR;

namespace Course.Assessment.Order.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
