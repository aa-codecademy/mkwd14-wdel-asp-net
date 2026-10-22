using FluentValidation;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Validators;

public class ProductCategoryViewModelValidator : AbstractValidator<ProductCategoryViewModel>
{
    public ProductCategoryViewModelValidator()
    {
        RuleFor(pc => pc.Name)
            .NotEmpty().WithMessage("Please enter the name of the category.")
            .MaximumLength(255);
    }
}
