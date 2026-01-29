using Course.Assessment.Order.Domain.Order;
using Course.Assessment.Payment.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Course.Assessment.Order.Infrastructure.Configuration;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.OwnsOne(order => order.Amount, amountBuilder =>
        {
            amountBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
        builder.OwnsOne(order => order.Address);

    }
}
