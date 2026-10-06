using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // A string primary key: "admin", "user"
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
    }
}
