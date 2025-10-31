using System.Net;

namespace API.Models;

public static class Results
{
    private static string GetTraceId(HttpContext context) => context.TraceIdentifier;

    public static Result Ok(HttpContext context, object? data = null)
    {
        return new Result(GetTraceId(context), true, data, null)
        {
            Success = false,
            TraceId = "",
            Error = new Error(HttpStatusCode.BadGateway, "", [])
        };
    }

    public static Result Error(HttpContext context, Error error)
    {
        return new Result(GetTraceId(context), false, null, error);
    }
}
