namespace Lamazon.ViewModels.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public decimal Price { get; set; }
    public bool IsFeatured { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal DiscountedPrice { get; set; }
    public bool IsAddedToCart { get; set; }
}
