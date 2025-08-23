using System.Text;
using Authentication.API.Data;
using Authentication.API.Interfaces;
using Authentication.API.Models.Enums;
using Authentication.API.Models.Options;
using Authentication.API.Repositories;
using Authentication.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using Share.Extensions;
using Share.Interceptors;
using Share.Options;

namespace Authentication.API.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddSerilogAndOpenTelemetry(
        this WebApplicationBuilder builder,
        IConfiguration configuration)
    {
        var host = builder.Host;
        var services = builder.Services;

        var otelConfiguration = services.ConfigureOptions<OpenTelemetryConfiguration>(configuration);

        host.UseSerilog((_, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Filter.ByExcluding(logEvent =>
                    logEvent.Properties.ContainsKey("RequestPath") &&
                    logEvent.Properties["RequestPath"].ToString().Contains("/metrics")
                )
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.GrafanaLoki(
                    otelConfiguration.LokiUrl,
                    [ new LokiLabel { Key = "service", Value = otelConfiguration.ServiceName } ]
                );
        });

        services.AddOpenTelemetry()
            .WithMetrics(metricsProvider =>
            {
                metricsProvider
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(otelConfiguration.ServiceName))
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelConfiguration.OtelUrl);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithTracing(tracerProvider =>
            {
                tracerProvider
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(otelConfiguration.ServiceName))
                    .AddSource(otelConfiguration.ServiceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = httpContext =>
                            !httpContext.Request.Path.Value!.StartsWith("/metrics") &&
                            !HttpMethods.IsOptions(httpContext.Request.Method);
                    })
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelConfiguration.OtelUrl);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHealthChecksBuilder healthChecksBuilder)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("DefaultConnection section not found");

        healthChecksBuilder.AddNpgSql(connectionString);
        services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationConfiguration = services.ConfigureOptions<AuthenticationConfiguration>(configuration);
        var authenticationOptions = authenticationConfiguration.AuthenticationOptions;
        var googleAuthenticationOptions = authenticationConfiguration.GoogleAuthenticationOptions;

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = AuthenticationSchemes.Token;
                options.DefaultChallengeScheme = AuthenticationSchemes.Google;
            })
            .AddJwtBearer(AuthenticationSchemes.Token, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authenticationOptions.Issuer,
                    ValidAudience = authenticationOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.Key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(AuthenticationSchemes.Token, out var token))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(AuthenticationSchemes.GoogleCookie)
            .AddGoogle(AuthenticationSchemes.Google, options =>
            {
                options.ClientId = googleAuthenticationOptions.ClientId;
                options.ClientSecret = googleAuthenticationOptions.ClientSecret;
                options.CallbackPath = "/google-callback";
                options.SignInScheme = AuthenticationSchemes.GoogleCookie;
            });

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository, TracingInterceptor>();
        services.AddTransient<IGoogleUserRepository, GoogleUserRepository, TracingInterceptor>();
        services.AddTransient<IUserService, UserService, TracingInterceptor>();

        return services;
    }
}