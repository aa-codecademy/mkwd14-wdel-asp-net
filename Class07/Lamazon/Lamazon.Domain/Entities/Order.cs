namespace Lamazon.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    public string? IpAddress { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryFlagUrl { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int OrderStatusId { get; set; }
    public OrderStatus OrderStatus { get; set; }

    public Invoice? Invoice { get; set; }

    public ICollection<OrderLineItem> OrderLineItems { get; set; } = [];
}
