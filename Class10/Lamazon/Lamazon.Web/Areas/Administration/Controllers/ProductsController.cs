using FluentValidation;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;
using Lamazon.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class ProductsController : AdminBaseController
{
    private readonly IProductsService _productsService;
    private readonly IProductCategoriesService _productCategoriesService;
    private readonly IValidator<ProductViewModel> _validator;

    public ProductsController(
        IProductsService productsService,
        IProductCategoriesService productCategoriesService,
        IValidator<ProductViewModel> validator)
    {
        _productsService = productsService;
        _productCategoriesService = productCategoriesService;
        _validator = validator;
        PageName = "Products";
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // The categories for the filter above the table
        await SetProductCategoriesAsync(cancellationToken);
        return View();
    }

    // Called by DataTables for every page, search, sort and category filter. Answers with JSON.
    [HttpPost]
    public async Task<IActionResult> GetProducts(
        ProductsDataTableRequestViewModel request,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _productsService.GetFilteredAsync(request, cancellationToken);
        return Json(pagedResult.ToDataTableResponse(request.Draw));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await SetProductCategoriesAsync(cancellationToken);
        return View(new ProductViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ProductViewModel productViewModel,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(productViewModel, cancellationToken);
        validationResult.AddToModelState(ModelState);
        if (!ModelState.IsValid)
        {
            // ViewBag lives for one request only: without this, the form comes back with an empty drop-down
            await SetProductCategoriesAsync(cancellationToken);
            return View(productViewModel);
        }

        await _productsService.CreateAsync(productViewModel, cancellationToken);
        AddSuccessNotification($"The product \"{productViewModel.Name}\" was created.");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var productViewModel = await _productsService.GetByIdAsync(id, cancellationToken);
        await SetProductCategoriesAsync(cancellationToken);
        return View(productViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductViewModel productViewModel, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(productViewModel, cancellationToken);
        validationResult.AddToModelState(ModelState);
        if (!ModelState.IsValid)
        {
            await SetProductCategoriesAsync(cancellationToken);
            return View(productViewModel);
        }

        await _productsService.UpdateAsync(productViewModel, cancellationToken);
        AddSuccessNotification($"The product \"{productViewModel.Name}\" was saved.");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var productViewModel = await _productsService.GetByIdAsync(id, cancellationToken);
        return View(productViewModel);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _productsService.DeleteAsync(id, cancellationToken);
        AddSuccessNotification("The product was deleted.");

        return RedirectToAction(nameof(Index));
    }

    // The options of the category drop-down. asp-for="ProductCategoryId" in the form selects the product's category
    private async Task SetProductCategoriesAsync(CancellationToken cancellationToken)
    {
        var productCategories = await _productCategoriesService.GetAllAsync(cancellationToken);
        ViewBag.ProductCategoryList = new SelectList(productCategories, "Id", "Name");
    }
}
