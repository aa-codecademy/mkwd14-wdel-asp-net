using Lamazon.DataAccess.Extensions;
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Context;

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LamazonDbContext).Assembly);

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
