namespace Lamazon.Domain.Entities;

public class OrderStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = [];
}
