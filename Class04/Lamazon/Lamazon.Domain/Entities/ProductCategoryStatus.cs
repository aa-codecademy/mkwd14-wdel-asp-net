namespace Lamazon.Domain.Entities;

public class ProductCategoryStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductCategory> ProductCategories { get; set; }
}
