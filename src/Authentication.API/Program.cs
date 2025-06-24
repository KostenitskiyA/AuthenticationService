using Authentication.API.Extensions;
using Authentication.API.Middlewares;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var healthChecksBuilder = builder.Services.AddHealthChecks();

// builder.Services.AddSerilog((_, config) =>
// {
//     config
//         .ReadFrom.Configuration(configuration)
//         .WriteTo.Console()
//         .WriteTo.GrafanaLoki("http://loki:3200");
// });

builder.Services.AddOpenTelemetry()
    // .WithTracing(tracerProvider =>
    // {
    //     tracerProvider
    //         .AddAspNetCoreInstrumentation()
    //         .AddHttpClientInstrumentation()
    //         .AddOtlpExporter(opt =>
    //         {
    //             opt.Endpoint = new Uri("http://tempo:4317");
    //         });
    // })
    .WithMetrics(metricsProvider =>
    {
        metricsProvider
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AuthenticationService"))
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();
    });

builder.Services.AddDatabase(configuration, healthChecksBuilder);
builder.Services.AddAuthentication(configuration);
builder.Services.AddServices();

if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Localhost", policy =>
        {
            policy.SetIsOriginAllowed(origin =>
                    origin.StartsWith("http://localhost") || 
                    origin.StartsWith("https://localhost"))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseCors("Localhost");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapControllers();

app.Run();