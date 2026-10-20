using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class HomeController : AdminBaseController
{
    public HomeController()
    {
        PageName = "Dashboard";
    }

    public IActionResult Index()
    {
        return View();
    }
}
