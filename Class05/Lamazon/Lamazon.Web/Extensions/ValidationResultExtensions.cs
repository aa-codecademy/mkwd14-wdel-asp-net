using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lamazon.Web.Extensions;

public static class ValidationResultExtensions
{
    /// <summary>
    /// Copies FluentValidation's errors into ModelState, so the view shows them
    /// next to the fields (asp-validation-for) and ModelState.IsValid becomes false.
    /// </summary>
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            // A field that already has an error (e.g. "This field is required." from model binding) keeps only that one
            if (modelState.TryGetValue(error.PropertyName, out var entry) && entry.Errors.Any())
            {
                continue;
            }

            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
