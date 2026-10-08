using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(order => order.TotalAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(order => order.IpAddress).HasMaxLength(45);
        builder.Property(order => order.CountryCode).HasMaxLength(5);
        builder.Property(order => order.CountryFlagUrl).HasMaxLength(255);

        // Two orders can never get the same number
        builder.HasIndex(order => order.OrderNumber).IsUnique();

        builder.HasOne(order => order.User)
            .WithMany(user => user.Orders)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(order => order.OrderStatus)
            .WithMany(orderStatus => orderStatus.Orders)
            .HasForeignKey(order => order.OrderStatusId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
