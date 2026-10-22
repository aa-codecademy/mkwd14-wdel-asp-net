using Lamazon.Domain.Constants;
using Lamazon.Web.Helpers;
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

    #region Notifications

    // Used to add success and error notifications to the TempData, which can be displayed in the views using the JS library 'notyf'

    protected void AddSuccessNotification(string message)
    {
        TempData[Notifications.MessageKey] = message;
        TempData[Notifications.TypeKey] = "success";
    }

    protected void AddErrorNotification(string message)
    {
        TempData[Notifications.MessageKey] = message;
        TempData[Notifications.TypeKey] = "error";
    }

    #endregion
}
