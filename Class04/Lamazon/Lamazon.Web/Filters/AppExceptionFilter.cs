using Lamazon.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lamazon.Web.Filters;

public class AppExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case NotFoundException notFound:
                // log info...
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                break;
        }
    }
}
