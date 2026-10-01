namespace Lamazon.Domain.Entities;

public class OrderLineItem : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal TotalPrice { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public ICollection<InvoiceLineItem> InvoiceLineItem { get; set; } = [];
}
