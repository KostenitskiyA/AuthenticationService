using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Application.Options;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Redis.Interfaces;
using AuthenticationOptions = Application.Options.AuthenticationOptions;
using ClaimTypes = Domain.Enums.ClaimTypes;

namespace Application.Services;

public class TokenService(
    IOptionsMonitor<AuthenticationConfiguration> authenticationConfigurationMonitor, 
    IRedisService redisService) 
    : ITokenService
{
    private readonly AuthenticationOptions _authenticationOptions =
        authenticationConfigurationMonitor.CurrentValue.AuthenticationOptions;
    
    public ClaimsPrincipal CreateUserClaims(User user)
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

    public string GenerateJwt(User user)
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

    public void AppendTokenCookie(HttpContext context, string key, string token, int expiresInMinutes)
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

    public async Task AuthenticationAsync(HttpContext context, User user)
    {
        var token = GenerateJwt(user);
        var refreshToken = Guid.NewGuid().ToString("N");

        AppendTokenCookie(context, AuthenticationSchemes.Token, token, _authenticationOptions.TokenExpiresInMinutes);
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
}