using Lamazon.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Controllers;

public abstract class BaseController : Controller
{
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

}
