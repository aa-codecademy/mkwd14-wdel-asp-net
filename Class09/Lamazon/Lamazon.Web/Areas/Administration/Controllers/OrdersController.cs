using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.DataTable;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class OrdersController : AdminBaseController
{
    private readonly IOrdersService _ordersService;

    public OrdersController(
        IOrdersService ordersService)
    {
        _ordersService = ordersService;
        PageName = "Orders";
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetOrders(
        DataTableRequestViewModel request,
        CancellationToken cancellationToken)
    {
        //var pagedResult = await _ordersService.

        return Json(null);
    }
}
