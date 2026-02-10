namespace Course.Assessment.Order.Domain.Order
{
    public interface IOrderRepository
    {
        Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Add(OrderEntity order);
    }
}
