using System.ComponentModel.DataAnnotations;

namespace Lamazon.ViewModels.Models;

public class UserCredentialsViewModel
{
    public string Email { get; set; } = string.Empty;
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
