using FluentValidation;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Validators;

public class ProductViewModelValidator : AbstractValidator<ProductViewModel>
{
    public ProductViewModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter the name of the product.")
            .MaximumLength(255);

        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Please enter a description.");

        RuleFor(p => p.ImageUrl)
            .NotEmpty().WithMessage("Please enter the address of the product's image.")
            .MaximumLength(2000)
            .Must(BeAWebAddress).WithMessage("Please enter a full web address, starting with http:// or https://.");

        RuleFor(p => p.ProductCategoryId)
            .GreaterThan(0).WithMessage("Please select a category.");

        // The Price column is decimal(10, 2): at most 8 digits before the decimal point
        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("The price must be more than 0.")
            .LessThan(100_000_000).WithMessage("The price must be less than 100,000,000.");

        RuleFor(p => p.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("The discount must be between 0 and 100 (%).");
    }

    private static bool BeAWebAddress(string imageUrl)
    {
        return Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
