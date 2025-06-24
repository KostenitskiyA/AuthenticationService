using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.API.Exceptions;
using Authentication.API.Interfaces;
using Authentication.API.Models.Dtos;
using Authentication.API.Models.Entities;
using Authentication.API.Models.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.API.Services;

public class UserService(
    IUserRepository userRepository,
    IOptionsMonitor<AuthenticationConfiguration> authenticationConfigurationMonitor
) : IUserService
{
    private readonly AuthenticationConfiguration _authenticationConfiguration =
        authenticationConfigurationMonitor.CurrentValue;

    private static ClaimsPrincipal CreateUserClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(nameof(ClaimTypes.Name), user.Name),
            new(nameof(ClaimTypes.Email), user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return claimsPrincipal;
    }
    
    private string GenerateJwt(User user)
    {
        var authenticationOptions = _authenticationConfiguration.AuthenticationOptions;

        var claims = CreateUserClaims(user).Claims;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: authenticationOptions.Issuer,
            audience: authenticationOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(authenticationOptions.ExpiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void AppendTokenCookie(HttpContext context, string token)
    {
        var options = _authenticationConfiguration.AuthenticationOptions;
        context.Response.Cookies.Append(
            JwtBearerDefaults.AuthenticationScheme,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(options.ExpiresInMinutes)
            });
    }

    public async Task<User> SignUpAsync(HttpContext context, SignInRequest request, CancellationToken ct)
    {
        var isUserExists = await userRepository.IsExistsAsync(user => user.Email == request.Email, ct);

        if (isUserExists)
            throw new DomainException($"User {request.Email} already exist");

        var user = await userRepository.AddAsync(
            new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = !string.IsNullOrEmpty(request.Password)
                    ? BCrypt.Net.BCrypt.HashPassword(request.Password)
                    : string.Empty
            }, ct);

        AppendTokenCookie(context, GenerateJwt(user));

        return user;
    }

    public async Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null)
            throw new DomainException($"User {request.Email} not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new DomainException($"User {request.Email} password invalid");

        AppendTokenCookie(context, GenerateJwt(user));

        return user;
    }

    public async Task LogOutAsync(HttpContext context, CancellationToken ct)
    {
        context.Response.Cookies.Delete(JwtBearerDefaults.AuthenticationScheme);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(HttpContext context, CancellationToken ct)
    {
        var email = context.User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            throw new DomainException("User not authorized");

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
            throw new DomainException($"User {email} not found");

        await userRepository.DeleteAsync(user, ct);
        await LogOutAsync(context, ct);
    }
}