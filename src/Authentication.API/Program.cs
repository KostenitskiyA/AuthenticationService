using Authentication.API.Extensions;
using Authentication.API.Middlewares;
using Redis.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var healthChecksBuilder = builder.Services.AddHealthChecks();

builder.AddSerilogAndOpenTelemetry(configuration);
builder.Services.AddRedis(configuration, healthChecksBuilder);
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

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoggerMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();