using Course.Assessment.Payment.Domain.Abstractions;
using MediatR;

namespace Course.Assessment.Payment.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
