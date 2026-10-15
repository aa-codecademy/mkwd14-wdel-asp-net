using Lamazon.DataAccess.Extensions;
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Context;

/// <summary>
/// The database session: one Table per table, and the model configuration.
/// </summary>
public class LamazonDbContext : DbContext
{
    public LamazonDbContext(DbContextOptions<LamazonDbContext> options) : base(options)
    {
    }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLineItem> InvoicesLineItems { get; set; }
    public DbSet<InvoiceStatus> InvoiceStatuses { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLineItem> OrderLineItems { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductStatus> ProductStatuses { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductCategoryStatus> ProductCategoriesStatus { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Every IEntityTypeConfiguration<T> class in this project (see the Configurations folder)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LamazonDbContext).Assembly);

        // Rows that must exist from the start: lookup tables, roles, the first users and some products
        modelBuilder
            .SeedProductCategoryStatuses()
            .SeedRoles()
            .SeedProductStatuses()
            .SeedProducts()
            .SeedUsers()
            .SeedProductCategories()
            .SeedOrderStatuses()
            .SeedInvoiceStatuses();
    }
}
