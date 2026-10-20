using System.ComponentModel.DataAnnotations;

namespace Lamazon.ViewModels.Models;

public class UserViewModel
{
    public int Id { get; set; }

    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public string RoleKey { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public string? RoleName { get; set; }
}
