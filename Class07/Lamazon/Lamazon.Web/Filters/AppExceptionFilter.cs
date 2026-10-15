using Lamazon.Domain.Exceptions;
using Lamazon.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Lamazon.Web.Filters;

public class AppExceptionFilter : IExceptionFilter
{
    private readonly ITempDataDictionaryFactory _tempDataFactory;

    // options.Filters.Add<AppExceptionFilter>() lets dependency injection create the filter, so the constructor can ask for services
    public AppExceptionFilter(ITempDataDictionaryFactory tempDataFactory)
    {
        _tempDataFactory = tempDataFactory;
    }

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case NotFoundException notFound:
                // log info...
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                break;

            case BusinessRuleException businessRule:
                // Back to the page the user came from, with the message in a red notification
                var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                tempData[Notifications.MessageKey] = businessRule.Message;
                tempData[Notifications.TypeKey] = "error";
                context.Result = new RedirectResult(GetPreviousPage(context.HttpContext.Request));
                context.ExceptionHandled = true;
                break;
        }
    }

    private static string GetPreviousPage(HttpRequest request)
    {
        // The Referer header is the page the user came from. Only follow it back to our own site.
        if (Uri.TryCreate(request.Headers.Referer.ToString(), UriKind.Absolute, out var referer)
            && referer.Host == request.Host.Host)
        {
            return referer.PathAndQuery;
        }

        return "/";
    }
}
