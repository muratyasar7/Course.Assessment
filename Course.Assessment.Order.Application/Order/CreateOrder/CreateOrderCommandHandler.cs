using Course.Assessment.Order.Application.Abstractions.Messaging;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order;

namespace Course.Assessment.Order.Application.Order.CreateOrder
{
    internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        public CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var order = OrderEntity.Create(Guid.NewGuid(), command.Request.CustomerId, command.Request.Amount, command.Request.Address, _dateTimeProvider.UtcNow);
            _orderRepository.Add(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(order.Id);
        }
    }
}
