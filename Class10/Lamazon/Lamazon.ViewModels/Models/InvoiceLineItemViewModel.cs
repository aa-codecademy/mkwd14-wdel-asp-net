namespace Lamazon.ViewModels.Models;

public class InvoiceLineItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ProductPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal TotalPrice { get; set; }
}
