using FluentValidation;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Validators;

public class RegisterUserViewModelValidator : AbstractValidator<RegisterUserViewModel>
{
    public RegisterUserViewModelValidator(IUsersRepository usersRepository)
    {
        RuleFor(u => u.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            //.MaximumLength(500).WithMessage("Full name must be less then 500 characters.")
            .MaximumLength(500);

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255)
            .MustAsync(async (email, cancellationToken) => !await usersRepository.EmailExistsAsync(email, cancellationToken)).WithMessage("An account with this email already exists.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(200);

        RuleFor(u => u.ConfirmPassword)
            .Equal(u => u.Password).WithMessage("The passwords don't match.");
    }
}
