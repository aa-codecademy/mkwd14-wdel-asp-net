using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using Lamazon.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lamazon.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductsService _productsService;

    public HomeController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<ProductViewModel> featuredProducts = await _productsService.GetFeaturedAsync(cancellationToken);
        return View(featuredProducts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
