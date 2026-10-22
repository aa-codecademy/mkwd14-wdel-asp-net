using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

public class ProductCategoryStatusConfiguration : IEntityTypeConfiguration<ProductCategoryStatus>
{
    public void Configure(EntityTypeBuilder<ProductCategoryStatus> builder)
    {
        builder.HasKey(x => x.Id);

        // Lookup table: we choose the ids ourselves (they match ProductCategoryStatusEnum), SQL Server doesn't generate them
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
    }
}
