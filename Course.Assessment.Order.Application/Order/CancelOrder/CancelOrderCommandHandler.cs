using Course.Assessment.Order.Application.Abstractions.Messaging;
using Course.Assessment.Order.Domain.Abstractions;

namespace Course.Assessment.Order.Application.Order.CancelOrder
{
    internal sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, Guid>
    {
        public Task<Result<Guid>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
