using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.DataTable;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

public class InvoicesController : AdminBaseController
{
    private readonly IInvoicesService _invoicesService;

    public InvoicesController(IInvoicesService invoicesService)
    {
        _invoicesService = invoicesService;
        PageName = "Invoices";
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetInvoices(
        DataTableRequestViewModel request,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _invoicesService.GetFilteredAsync(request, cancellationToken);
        return Json(pagedResult.ToDataTableResponse(request.Draw));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var invoiceViewModel = await _invoicesService.GetByIdAsync(id, cancellationToken);
        return View(invoiceViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAsPaid(int id, CancellationToken cancellationToken)
    {
        await _invoicesService.SetAsPaidAsync(id, cancellationToken);
        AddSuccessNotification("The invoice was marked as paid.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _invoicesService.CancelAsync(id, cancellationToken);
        AddSuccessNotification("The invoice was canceled.");
        return RedirectToAction(nameof(Index));
    }
}
