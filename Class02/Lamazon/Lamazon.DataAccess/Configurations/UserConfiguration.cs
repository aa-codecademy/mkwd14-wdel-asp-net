using System;
using System.Collections.Generic;
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(255);
        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(user => user.FullName).IsRequired().HasMaxLength(500);
        builder.Property(user => user.RoleKey).IsRequired().HasMaxLength(50);

        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasOne(user => user.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(user => user.RoleKey)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
