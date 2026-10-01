using System.ComponentModel.DataAnnotations;

namespace Lamazon.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsFeatured { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal DiscountedPrice => Math.Round((1 - DiscountPercentage / 100) * Price, 2);

    public int ProductCategoryId { get; set; }
    public ProductCategory ProductCategory { get; set; }

    public int ProductStatusId { get; set; }
    public ProductStatus ProductStatus { get; set; }

    public ICollection<InvoiceLineItem> InvoiceLineItems { get; set; } = [];
    public ICollection<OrderLineItem> OrderLineItems { get; set; } = [];
}
