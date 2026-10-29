using FluentValidation;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;
using Lamazon.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class ProductCategoriesController : AdminBaseController
{
    private readonly IProductCategoriesService _productCategoriesService;
    private readonly IValidator<ProductCategoryViewModel> _validator;

    public ProductCategoriesController(
        IProductCategoriesService productCategoriesService,
        IValidator<ProductCategoryViewModel> validator)
    {
        _productCategoriesService = productCategoriesService;
        _validator = validator;
        PageName = "Product Categories";
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetProductCategories(
        DataTableRequestViewModel request,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _productCategoriesService.GetFilteredAsync(request, cancellationToken);
        return Json(pagedResult.ToDataTableResponse(request.Draw));
    }

    public IActionResult Create()
    {
        return View(new ProductCategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ProductCategoryViewModel productCategoryViewModel,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(productCategoryViewModel, cancellationToken);
        validationResult.AddToModelState(ModelState);
        if (!ModelState.IsValid)
        {
            return View(productCategoryViewModel);
        }

        await _productCategoriesService.CreateAsync(productCategoryViewModel, cancellationToken);
        AddSuccessNotification($"The category \"{productCategoryViewModel.Name}\" was created.");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var productCategoryVM = await _productCategoriesService.GetByIdAsync(id, cancellationToken);
        return View(productCategoryVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductCategoryViewModel productCategoryViewModel, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(productCategoryViewModel, cancellationToken);
        validationResult.AddToModelState(ModelState);
        if (!ModelState.IsValid)
        {
            return View(productCategoryViewModel);
        }

        await _productCategoriesService.UpdateAsync(productCategoryViewModel, cancellationToken);
        AddSuccessNotification($"The category \"{productCategoryViewModel.Name}\" was saved.");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var productCategoryViewModel = await _productCategoriesService.GetByIdAsync(id, cancellationToken);
        return View(productCategoryViewModel);
    }

    // Two actions can't both be Delete(int id): C# needs different names, [ActionName] gives them the same URL
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _productCategoriesService.DeleteAsync(id, cancellationToken);
        AddSuccessNotification("The category was deleted.");

        return RedirectToAction(nameof(Index));
    }
}
