using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Controllers;

public class ProductsController : BaseController
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<ProductViewModel> products = await _productsService.GetAllAsync(cancellationToken);
        MarkProductsInCart(products);

        return View(products);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        // IMPORTANT: Instead of using try-catch on every action, we are using an global ApplicationExceptionFilter to handle exceptions and redirect to the Error page.
        // This keeps the code cleaner and more maintainable.

        //try
        //{
        ProductViewModel product = await _productsService.GetByIdAsync(id, cancellationToken);
        MarkProductsInCart([product]);

        return View(product);
        //}
        //catch (Exception)
        //{
        //    return RedirectToAction("Error", controllerName: "Home");
        //}
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, CancellationToken cancellationToken)
    {
        _ = await _productsService.GetByIdAsync(productId, cancellationToken);

        var shoppingCart = GetShoppingCart();

        if (!shoppingCart.Items.Any(item => item.ProductId == productId))
        {
            shoppingCart.Items.Add(new() { ProductId = productId });
            SaveShoppingCart(shoppingCart);
        }

        return Json(new { itemsCount = shoppingCart.Items.Count });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int productId)
    {
        var shoppingCart = GetShoppingCart();
        shoppingCart.Items.RemoveAll(item => item.ProductId == productId);
        SaveShoppingCart(shoppingCart);

        return Json(new { itemsCount = shoppingCart.Items.Count });
    }

}
