namespace Lamazon.Domain.Entities;

public class ProductStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];
}
