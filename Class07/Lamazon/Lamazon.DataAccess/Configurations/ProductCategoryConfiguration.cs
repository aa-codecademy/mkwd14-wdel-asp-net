using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

internal class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pc => pc.ProductCategoryStatusId)
            .HasDefaultValue((int)ProductCategoryStatusEnum.Active);

        builder.HasOne(pc => pc.ProductCategoryStatus)
            .WithMany(status => status.ProductCategories)
            .HasForeignKey(pc => pc.ProductCategoryStatusId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
