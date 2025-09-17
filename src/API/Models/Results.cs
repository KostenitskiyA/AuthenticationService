namespace API.Models;

public static class Results
{
    private static string GetTraceId(HttpContext context)
    {
        return context.TraceIdentifier;
    }

    public static Result Ok(HttpContext context, object? data = null)
    {
        return new Result(GetTraceId(context), true, data, null);
    }

    public static Result Error(HttpContext context, Error error)
    {
        return new Result(GetTraceId(context), false, null, error);
    }
}