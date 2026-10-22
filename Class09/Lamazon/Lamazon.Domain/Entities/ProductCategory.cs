namespace Lamazon.Domain.Entities;

public class ProductCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int ProductCategoryStatusId { get; set; }
    public ProductCategoryStatus ProductCategoryStatus { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
