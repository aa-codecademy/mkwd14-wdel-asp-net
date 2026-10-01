using System;
using System.Collections.Generic;
using System.Text;
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

internal class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(invoice => invoice.TotalAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();

        builder.HasOne(invoice => invoice.User)
            .WithMany(invoice => invoice.Invoices)
            .HasForeignKey(invoice => invoice.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(invoice => invoice.Order)
            .WithOne(order => order.Invoice)
            .HasForeignKey<Invoice>(invoice => invoice.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(invoice => invoice.InvoiceStatus)
            .WithMany(status => status.Invoices)
            .HasForeignKey(invoice => invoice.InvoiceStatusId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
