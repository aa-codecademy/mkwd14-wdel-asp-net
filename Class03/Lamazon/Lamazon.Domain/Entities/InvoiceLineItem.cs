namespace Lamazon.Domain.Entities;

public class InvoiceLineItem : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal TotalPrice { get; set; }

    // Relations
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; }

    public int OrderLineItemId { get; set; }
    public OrderLineItem OrderLineItem { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}
