using System.Runtime.InteropServices;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Fluent API way of configuring 
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(product => product.Description)
            .HasMaxLength(10000)
            .IsRequired();

        builder.Property(product => product.ImageUrl)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        // Default values for rows inserted without them (for example straight from SQL)
        builder.Property(product => product.ProductStatusId)
            .HasDefaultValue((int)ProductStatusEnum.Active);
        builder.Property(product => product.IsFeatured)
            .HasDefaultValue(false);
        builder.Property(product => product.DiscountPercentage)
            .HasDefaultValue(0);

        // Relations
        builder.HasOne(product => product.ProductCategory)
            .WithMany(productCategory => productCategory.Products)
            .HasForeignKey(product => product.ProductCategoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Product_ProductCategory");

        builder.HasOne(product => product.ProductStatus)
            .WithMany(productStatus => productStatus.Products)
            .HasForeignKey(product => product.ProductStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Product_ProductStatus");
    }
}
