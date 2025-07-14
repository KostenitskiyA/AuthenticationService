using Authentication.API.Models.Enums;
using Serilog.Context;

namespace Authentication.API.Middlewares;

public class LoggerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var requestId = Guid.NewGuid().ToString();
        var userId = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Id)?.Value ?? "unknown";
        
        using (LogContext.PushProperty("IP", ip))
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("UserId", userId))
        {
            await next(context);
        }
    }
}