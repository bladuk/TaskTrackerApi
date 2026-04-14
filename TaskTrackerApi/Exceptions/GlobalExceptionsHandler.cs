using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerApi.Exceptions;

public class GlobalExceptionsHandler(ILogger<GlobalExceptionsHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred. Trace ID: {TraceId}", httpContext.TraceIdentifier);

        var (statusCode, title) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = GetExceptionMessageSafe(exception, httpContext)
            }, cancellationToken);

        return true;
    }

    private (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        AppException appException => (appException.StatusCode, appException.Title),
        ValidationException => (StatusCodes.Status400BadRequest, "Bad Request"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };

    private string? GetExceptionMessageSafe(Exception exception, HttpContext context)
    {
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            return exception.Message;
        }

        return exception is AppException ? exception.Message : null;
    }
}