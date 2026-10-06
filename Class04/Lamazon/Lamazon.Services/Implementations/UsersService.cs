using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Implementations;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }


    public Task<UserViewModel> RegisterAsync(RegisterViewModel registerViewModel, CancellationToken cancellationToken)
    {

    }

    public Task<UserViewModel?> ValidateCredentialsAsync(UserCredentialsViewModel userCredentials)
    {

    }
}
