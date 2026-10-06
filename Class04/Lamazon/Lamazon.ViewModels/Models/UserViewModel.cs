namespace Lamazon.ViewModels.Models;

public class UserViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleKey { get; set; } = string.Empty;
    public string? RoleName { get; set; }
}
