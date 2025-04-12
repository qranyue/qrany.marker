using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Marker.WebApi;

/// <summary>
/// 错误处理
/// </summary>
/// <param name="logger"></param>
public class ExceptionFilter(ILogger<ExceptionFilter> logger) : IExceptionFilter
{
    /// <summary>
    /// 错误处理
    /// </summary>
    /// <param name="context"></param>
    public void OnException(ExceptionContext context)
    {
        logger.LogWarning("{} --- {}\r\n{}\r\n{}\r\n\r\n\r\n", context.HttpContext.Request.Path, DateTime.Now, context.Exception.Message, context.Exception.StackTrace);
        context.Result = new ObjectResult(new RE(context.Exception.Message));
        context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.ExceptionHandled = true;
    }
}