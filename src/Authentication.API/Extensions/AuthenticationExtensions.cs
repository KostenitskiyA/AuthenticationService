using Authentication.API.Data;
using Authentication.API.Models.Options;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(AuthenticationConfiguration));
        if (section is null)
            throw new Exception($"{nameof(AuthenticationConfiguration)} section not found");
        
        var authenticationConfiguration = section.Get<AuthenticationConfiguration>();
        if (authenticationConfiguration is null)
            throw new Exception($"{nameof(AuthenticationConfiguration)} options not found");

        var authenticationOptions = authenticationConfiguration.AuthenticationOptions;
        var googleAuthenticationOptions = authenticationConfiguration.GoogleAuthenticationOptions;
        
        services.Configure<AuthenticationConfiguration>(section);
        services.AddAuthentication(authenticationOptions.TokenName)
            .AddCookie(authenticationOptions.TokenName, options =>
            {
                options.Cookie.Name = authenticationOptions.TokenName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(authenticationOptions.ExpiresInMinutes);
            })
            .AddGoogle(googleAuthenticationOptions.TokenName, options =>
            {
                options.ClientId = googleAuthenticationOptions.ClientId;
                options.ClientSecret = googleAuthenticationOptions.ClientSecret;
                options.CallbackPath = "/signin-google";
            });

        return services;
    }
    
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception();
        
        services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString));
        
        return services;
    }
}