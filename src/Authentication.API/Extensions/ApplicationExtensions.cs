﻿using System.Text;
using Authentication.API.Data;
using Authentication.API.Interfaces;
using Authentication.API.Models.Options;
using Authentication.API.Repositories;
using Authentication.API.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace Authentication.API.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddOTEL(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var host = builder.Host;
        var services = builder.Services;

        var otelConfiguration = services.ConfigureOptions<OpenTelemetryConfiguration>(configuration);

        host.UseSerilog((_, _, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.GrafanaLoki(
                    otelConfiguration.LokiUrl,
                    [new LokiLabel { Key = "app", Value = otelConfiguration.ServiceName }]
                );
        });

        services.AddOpenTelemetry()
            .WithTracing(tracerProvider =>
            {
                tracerProvider
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(otelConfiguration.ServiceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => { options.Endpoint = new Uri(otelConfiguration.TempoUrl); });
            })
            .WithMetrics(metricsProvider =>
            {
                metricsProvider
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(otelConfiguration.ServiceName))
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationConfiguration = services.ConfigureOptions<AuthenticationConfiguration>(configuration);
        var authenticationOptions = authenticationConfiguration.AuthenticationOptions;
        var googleAuthenticationOptions = authenticationConfiguration.GoogleAuthenticationOptions;

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
                        if (context.Request.Cookies.TryGetValue(JwtBearerDefaults.AuthenticationScheme, out var token))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = googleAuthenticationOptions.ClientId;
                options.ClientSecret = googleAuthenticationOptions.ClientSecret;
                options.CallbackPath = "/google-callback";
                options.SignInScheme = "GoogleToken";
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

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IUserService, UserService>();

        return services;
    }
}