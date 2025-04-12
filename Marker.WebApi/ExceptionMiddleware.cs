using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Marker.WebApi;

public class ExceptionFilter(ILogger<ExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        logger.LogWarning("{} --- {}\r\n{}\r\n{}\r\n\r\n\r\n", context.HttpContext.Request.Path, DateTime.Now, context.Exception.Message, context.Exception.StackTrace);
        context.Result = new ObjectResult(new RE(context.Exception.Message));
        context.ExceptionHandled = true;
    }
}