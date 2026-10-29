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
        var pagedResult = await _ordersService.GetFilteredAsync(request, cancellationToken);
        return Json(pagedResult.ToDataTableResponse(request.Draw));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var orderViewModel = await _ordersService.GetByIdAsync(id, cancellationToken);
        return View(orderViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id, CancellationToken cancellationToken)
    {
        await _ordersService.AcceptAsync(id, cancellationToken);
        AddSuccessNotification("The order was accepted and its invoice was created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        // Reject the order

        return RedirectToAction(nameof(Index));
    }

}
