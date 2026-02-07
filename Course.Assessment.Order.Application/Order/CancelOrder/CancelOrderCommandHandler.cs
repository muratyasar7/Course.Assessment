using Course.Assessment.Order.Application.Abstractions.Messaging;
using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order;

namespace Course.Assessment.Order.Application.Order.CancelOrder
{
    internal sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result.Failure<Guid>(Error.NullValue);
            }
            order.CancelOrder();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(order.Id);
        }
    }
}
