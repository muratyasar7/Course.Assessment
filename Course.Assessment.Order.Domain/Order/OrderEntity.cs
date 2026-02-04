using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order.Events;
using Course.Assessment.Order.Domain.Shared;
using Course.Assessment.Payment.Domain.Shared;

namespace Course.Assessment.Order.Domain.Order
{
    public sealed class OrderEntity : Entity
    {
        private OrderEntity(
        Guid id,
        Guid customerId,
        Money amount,
        Address address,
        DateTime createdAt)
        : base(id)
        {
            CustomerId = customerId;
            Amount = amount ?? throw new System.ArgumentNullException(nameof(amount));
            Address = address ?? throw new System.ArgumentNullException(nameof(address));
            CreatedAt = createdAt;
        }
        private OrderEntity()
        {
            Amount = default!;
            Address = default!;
        }
        public Guid CustomerId { get; private set; }
        public Money Amount { get; private set; }
        public Address Address { get; private set; }
        public DateTime CreatedAt { get; set; }



        public static OrderEntity Create(Guid id, Guid customerId, Money amount, Address address, DateTime createdAt)
        {
            var newOrder = new OrderEntity(id, customerId, amount, address, createdAt);
            newOrder.RaiseDomainEvent(new OrderCreatedDomainEvent(id, amount.Amount, amount.Currency.Code));
            return newOrder;
        }
    }
}
