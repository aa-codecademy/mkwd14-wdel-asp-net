using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.DataTable;
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

    [HttpPost]
    public async Task<IActionResult> GetProductCategories(
        DataTableRequestViewModel request,
        CancellationToken cancellationToken)
    {


        return Json(null);
    }
}
