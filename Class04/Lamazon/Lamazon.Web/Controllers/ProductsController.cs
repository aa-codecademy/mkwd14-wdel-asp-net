using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<ProductViewModel> products = await _productsService.GetAllAsync(cancellationToken);

        return View(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        //try
        //{
        ProductViewModel product = await _productsService.GetByIdAsync(id, cancellationToken);

        return View(product);
        //}
        //catch (Exception)
        //{
        //    return RedirectToAction("Error", controllerName: "Home");
        //}
    }
}
