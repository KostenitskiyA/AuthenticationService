using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.Models;
using Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Serilog;

namespace API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private static Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        IEnumerable<ValidationFailure>? details,
        CancellationToken ct)
    {
        var errorDetails = details?.Select(detail => new ValidationError(detail.PropertyName, detail.ErrorMessage))
            .ToArray() ?? [];

        var result = Result.Failure(context.TraceIdentifier, new Error(statusCode, message, errorDetails));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(
            result,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = false
            }
        );

        return context.Response.WriteAsync(json, ct);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var ct = context.RequestAborted;

        try
        {
            await next(context);
        }
        catch (NotAuthorizedException exception)
        {
            Log.Information("{TraceId} - {Message}", traceId, exception.Message);
            await HandleExceptionAsync(context, exception.StatusCode, exception.Message, null, ct);
        }
        catch (ValidationException exception)
        {
            Log.Information("{TraceId} - {Message}", traceId, exception.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "Validation error", exception.Errors, ct);
        }
        catch (EntityNotFoundException exception)
        {
            Log.Information("{TraceId} - {Message}", traceId, exception.Message);
            await HandleExceptionAsync(context, exception.StatusCode, exception.Message, null, ct);
        }
        catch (DomainException exception)
        {
            Log.Information("{TraceId} - {Message}", traceId, exception.Message);
            await HandleExceptionAsync(context, exception.StatusCode, exception.Message, null, ct);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "{TraceId} - {Message}", traceId, "An unhandled exception occurred");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Server error", null, ct);
        }
    }
}
