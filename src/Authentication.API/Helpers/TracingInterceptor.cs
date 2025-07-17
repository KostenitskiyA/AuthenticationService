using System.Diagnostics;
using Authentication.API.Models.Options;
using Castle.DynamicProxy;
using Microsoft.Extensions.Options;

namespace Authentication.API.Helpers;

public class TracingInterceptor(IOptions<OpenTelemetryConfiguration> options) : IInterceptor
{
    private readonly ActivitySource ActivitySource = new(options.Value.ServiceName);

    public void Intercept(IInvocation invocation)
    {
        using var activity = ActivitySource.StartActivity(invocation.Method.Name);
        invocation.Proceed();
    }
}