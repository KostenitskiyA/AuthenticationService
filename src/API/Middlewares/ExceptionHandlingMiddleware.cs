using System.Net;
using System.Text.Json;
using API.Models;
using Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Serilog;
using Results = API.Models.Results;

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
        var result = Results.Error(
            context,
            new Error(
                statusCode,
                message,
                details!.Select(detail =>
                    new ValidationError(detail.PropertyName, detail.ErrorMessage))
                    .ToArray()));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(result);
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
        catch (NotAuthorizedException ex)
        {
            Log.Information("{TraceId} - {Message}", traceId, ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message, null, ct);
        }
        catch (ValidationException ex)
        {
            Log.Information("{TraceId} - {Message}", traceId, ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, nameof(ValidationError), ex.Errors, ct);
        }
        catch (EntityNotFoundException ex)
        {
            Log.Information("{TraceId} - {Message}", traceId, ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message, null, ct);
        }
        catch (DomainException ex)
        {
            Log.Information("{TraceId} - {Message}", traceId, ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message, null, ct);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "{TraceId} - {Message}", traceId, "An unhandled exception occurred");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Server error", null, ct);
        }
    }
}