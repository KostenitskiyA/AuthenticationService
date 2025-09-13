using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using Serilog;

namespace API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string? content = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return content is null
            ? Task.CompletedTask
            : context.Response.WriteAsync(content);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotAuthorizedException exception)
        {
            Log.Error(exception, exception.Message);
            await HandleExceptionAsync(
                context,
                exception.StatusCode,
                JsonSerializer.Serialize(new { message = exception.Message }));
        }
        catch (EntityNotFoundException exception)
        {
            Log.Error(exception, exception.Message);
            await HandleExceptionAsync(
                context,
                exception.StatusCode,
                JsonSerializer.Serialize(new { message = exception.Message }));
        }
        catch (DomainException exception)
        {
            Log.Error(exception, exception.Message);
            await HandleExceptionAsync(
                context,
                exception.StatusCode,
                JsonSerializer.Serialize(new { message = exception.Message }));
        }
        catch (Exception exception)
        {
            Log.Error(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError);
        }
    }
}