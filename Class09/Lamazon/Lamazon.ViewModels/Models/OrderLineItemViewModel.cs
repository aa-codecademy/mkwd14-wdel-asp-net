namespace Lamazon.ViewModels.Models;

public class OrderLineItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>The price of one item, with the discount already applied.</summary>
    public decimal ProductPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal TotalPrice { get; set; }

    // Only shown in the shopping cart, not saved with the order
    public string? ProductDescription { get; set; }
    public string? ProductImageUrl { get; set; }
}
