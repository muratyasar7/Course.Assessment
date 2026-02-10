using Course.Assessment.Payment.Domain.Abstractions;
using MediatR;

namespace Course.Assessment.Payment.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
