using Lamazon.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class ProductCategoriesController : AdminBaseController
{
    private readonly IProductCategoriesService _productCategoriesService;

    public ProductCategoriesController(IProductCategoriesService productCategoriesService)
    {
        _productCategoriesService = productCategoriesService;
        PageName = "Product Categories";
    }

    public IActionResult Index()
    {
        return View();
    }
}
