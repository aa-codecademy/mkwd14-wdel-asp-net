using FluentValidation;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Validators;

public class UserCredentialsViewModelValidator : AbstractValidator<UserCredentialsViewModel>
{
    // No database rule: whether the account exists is the service's answer (the same one for a wrong email and a wrong password)
    public UserCredentialsViewModelValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
