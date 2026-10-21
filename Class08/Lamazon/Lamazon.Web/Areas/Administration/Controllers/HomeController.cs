using Lamazon.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class HomeController : AdminBaseController
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        PageName = "Dashboard";
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dashboardData = await _dashboardService.GetDashboardDataAsync(cancellationToken);
        return View(dashboardData);
    }
}
