using Lamazon.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lamazon.Web.Areas.Administration.Controllers;

[Area("Administration")] // asp-area="Administration" in the view
[Authorize(Roles = Roles.Admin)]
public abstract class AdminBaseController : Controller
{
    protected string PageName { get; set; } = string.Empty;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.PageName = PageName;
        base.OnActionExecuting(context);
    }
}
