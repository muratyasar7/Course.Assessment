
using Course.Assessment.Payment.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Course.Assessment.Payment.Infrastructure.Configuration;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.PaymentReference).IsRequired();
        builder.Property(payment => payment.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(payment => payment.Currency).IsRequired().HasMaxLength(3);
        builder.Property(payment => payment.Status).IsRequired();
    }
}
