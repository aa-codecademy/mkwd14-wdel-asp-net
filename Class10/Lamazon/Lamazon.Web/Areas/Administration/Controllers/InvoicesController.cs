using Lamazon.Services.Abstractions;
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
}
