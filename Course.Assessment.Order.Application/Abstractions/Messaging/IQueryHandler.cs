using Course.Assessment.Order.Domain.Abstractions;
using MediatR;

namespace Course.Assessment.Order.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
