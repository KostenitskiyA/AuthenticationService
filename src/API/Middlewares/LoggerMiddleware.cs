using Domain.Enums;
using Serilog.Context;

namespace API.Middlewares;

public class LoggerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/metrics"))
        {
            await next(context);
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Id)?.Value ?? "unknown";

        using (LogContext.PushProperty("IP", ip))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            await next(context);
        }
    }
}