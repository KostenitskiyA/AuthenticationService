﻿using System.Net;
using System.Text.Json;
using Authentication.API.Exceptions;

namespace Authentication.API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (DomainException exception)
        {
            logger.LogError(exception, exception.Message);
            var content = JsonSerializer.Serialize(new { message = exception.Message });
            await HandleExceptionAsync(context, exception.StatusCode, content);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError);
        }
    }
}