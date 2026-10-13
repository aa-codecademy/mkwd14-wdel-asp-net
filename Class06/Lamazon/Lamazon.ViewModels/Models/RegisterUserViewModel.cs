using System.ComponentModel.DataAnnotations;

namespace Lamazon.ViewModels.Models;

public class RegisterUserViewModel
{
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [DataType(DataType.Password)]
    public string Password { get; set;  } = string.Empty;
    [Display(Name = "Confirm password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set;  } = string.Empty;
}
