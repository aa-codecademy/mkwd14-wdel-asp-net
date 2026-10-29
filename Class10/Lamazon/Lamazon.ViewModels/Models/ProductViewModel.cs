using System.ComponentModel.DataAnnotations;

namespace Lamazon.ViewModels.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Display(Name = "Image URL")]
    public string ImageUrl { get; set; } = string.Empty;
    [Display(Name = "Category")]
    public int ProductCategoryId { get; set; }
    [Display(Name = "Category")]
    public string? ProductCategoryName { get; set; }
    public decimal Price { get; set; }
    [Display(Name = "Featured")]
    public bool IsFeatured { get; set; }
    [Display(Name = "Discount (%)")]
    public int DiscountPercentage { get; set; }
    public decimal DiscountedPrice { get; set; }
    public bool IsAddedToCart { get; set; }
}
