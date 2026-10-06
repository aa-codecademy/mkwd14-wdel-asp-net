using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Constants;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Exceptions;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;

namespace Lamazon.Services.Implementations;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersService(
        IUsersRepository usersRepository,
        IMapper mapper,
        IPasswordHasher<User> passwordHasher)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserViewModel> RegisterAsync(RegisterViewModel registerViewModel, CancellationToken cancellationToken)
    {
        User user = _mapper.Map<User>(registerViewModel);

        // Everyone who registers is a customer. Only an admin can make someone an admin.
        user.RoleKey = Roles.User;

        // Never store the password itself: only a salted hash
        user.PasswordHash = _passwordHasher.HashPassword(user, registerViewModel.Password);

        await _usersRepository.AddAsync(user, cancellationToken);

        // Load it again together with its role, so the view model is complete
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<UserViewModel?> ValidateCredentialsAsync(UserCredentialsViewModel userCredentials, CancellationToken cancellationToken)
    {
        User? user = await _usersRepository.GetByEmailAsync(userCredentials.Email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userCredentials.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        // The password is right, but it was hashed with older (weaker) settings: hash it again with the current ones
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, userCredentials.Password);
            await _usersRepository.UpdateAsync(user, cancellationToken);
        }

        return _mapper.Map<UserViewModel>(user);
    }

    public async Task<UserViewModel> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        User user = await _usersRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException(nameof(User), id);

        return _mapper.Map<UserViewModel>(user);
    }
}
