using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Application.Options;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Redis.Interfaces;
using AuthenticationOptions = Application.Options.AuthenticationOptions;
using ClaimTypes = Domain.Enums.ClaimTypes;

namespace Application.Services;

public class TokenService(
    IOptionsMonitor<AuthenticationConfiguration> authenticationConfigurationMonitor,
    IUserRepository userRepository,
    IRedisService redisService)
    : ITokenService
{
    private readonly AuthenticationOptions _authenticationOptions =
        authenticationConfigurationMonitor.CurrentValue.AuthenticationOptions;

    public async Task AppendTokensAsync(HttpContext context, User user)
    {
        var token = GenerateJwt(user);
        var refreshToken = Guid.NewGuid().ToString("N");

        AppendTokenCookie(
            context,
            AuthenticationSchemes.Token,
            token,
            _authenticationOptions.TokenExpiresInMinutes);
        AppendTokenCookie(
            context,
            AuthenticationSchemes.RefreshToken,
            refreshToken,
            _authenticationOptions.RefreshTokenExpiresInMinutes);

        await redisService.SetStringAsync(
            $"authentication:refresh:{refreshToken}",
            user.Id,
            expireTime: TimeSpan.FromMinutes(_authenticationOptions.RefreshTokenExpiresInMinutes));
    }

    public async Task RefreshTokensAsync(HttpContext context, CancellationToken ct)
    {
        var refreshToken = context.Request.Cookies[AuthenticationSchemes.RefreshToken];

        if (string.IsNullOrEmpty(refreshToken))
            return;

        var userId = await redisService.GetStringAsync<string>($"authentication:refresh:{refreshToken}");

        await RevokeTokensAsync(context);

        if (Guid.TryParse(userId, out var id))
        {
            var user = await userRepository.GetByIdAsync(id, ct);

            if (user is null)
                throw new EntityNotFoundException(nameof(User), userId);

            await AppendTokensAsync(context, user);
        }
    }

    public async Task RevokeTokensAsync(HttpContext context)
    {
        var refreshToken = context.Request.Cookies[AuthenticationSchemes.RefreshToken];

        if (!string.IsNullOrEmpty(refreshToken))
            await redisService.DeleteKeyAsync($"authentication:refresh:{refreshToken}");

        context.Response.Cookies.Delete(AuthenticationSchemes.Token);
        context.Response.Cookies.Delete(AuthenticationSchemes.RefreshToken);
    }

    private string GenerateJwt(User user)
    {
        var claims = CreateUserClaims(user).Claims;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _authenticationOptions.Issuer,
            _authenticationOptions.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_authenticationOptions.TokenExpiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static ClaimsPrincipal CreateUserClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Id, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        };

        var claimsIdentity = new ClaimsIdentity(claims, AuthenticationSchemes.Token);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return claimsPrincipal;
    }

    private static void AppendTokenCookie(HttpContext context, string key, string token, int expiresInMinutes)
    {
        context.Response.Cookies.Append(
            key,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            });
    }
}