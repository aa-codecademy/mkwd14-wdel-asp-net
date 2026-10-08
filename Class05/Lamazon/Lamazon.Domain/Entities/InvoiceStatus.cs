namespace Lamazon.Domain.Entities;

public class InvoiceStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Invoice> Invoices { get; set; } = [];
}
