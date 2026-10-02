namespace Lamazon.Domain.Entities;

public class Role
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
