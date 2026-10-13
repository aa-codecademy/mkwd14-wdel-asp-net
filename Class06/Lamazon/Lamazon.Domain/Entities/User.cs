namespace Lamazon.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? Age { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string RoleKey { get; set; } = string.Empty;
    public Role Role { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
