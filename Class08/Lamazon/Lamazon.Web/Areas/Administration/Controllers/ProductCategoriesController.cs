using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class ProductCategoriesController : AdminBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
