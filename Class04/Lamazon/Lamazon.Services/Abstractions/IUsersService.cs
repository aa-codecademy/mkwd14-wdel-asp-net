using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IUsersService
{
    Task<UserViewModel> RegisterAsync(RegisterViewModel registerViewModel, CancellationToken cancellationToken);
    Task<UserViewModel?> ValidateCredentialsAsync(UserCredentialsViewModel userCredentials, CancellationToken cancellationToken);
    Task<UserViewModel> GetByIdAsync(int id, CancellationToken cancellationToken);
}
