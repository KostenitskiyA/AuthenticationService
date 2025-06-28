using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.API.Exceptions;
using Authentication.API.Interfaces;
using Authentication.API.Models.Dtos;
using Authentication.API.Models.Entities;
using Authentication.API.Models.Enums;
using Authentication.API.Models.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Redis.Interfaces;
using ClaimTypes = Authentication.API.Models.Enums.ClaimTypes;

namespace Authentication.API.Services;

public class UserService(
    IOptionsMonitor<AuthenticationConfiguration> authenticationConfigurationMonitor,
    IUserRepository userRepository,
    IRedisService redisService
) : IUserService
{
    private readonly AuthenticationOptions _authenticationOptions =
        authenticationConfigurationMonitor.CurrentValue.AuthenticationOptions;

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

        await Authentication(context, user);

        return user;
    }

    public async Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null)
            throw new DomainException($"User {request.Email} not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new DomainException($"User {request.Email} password invalid");

        await Authentication(context, user);

        return user;
    }

    public async Task LogOutAsync(HttpContext context, CancellationToken ct)
    {
        context.Response.Cookies.Delete(AuthenticationSchemes.Token);
        context.Response.Cookies.Delete(AuthenticationSchemes.RefreshToken);

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(HttpContext context, CancellationToken ct)
    {
        var email = context.User.FindFirstValue(ClaimTypes.Email);

        if (context.User.Identity is not { IsAuthenticated: true } || string.IsNullOrEmpty(email))
            throw new DomainException("User not authorized");

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
            throw new DomainException($"User {email} not found");

        await userRepository.DeleteAsync(user, ct);
        await LogOutAsync(context, ct);
    }

    public async Task RefreshAsync(HttpContext context, CancellationToken ct)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
            await LogOutAsync(context, ct);

        var userId = context.User.FindFirstValue(ClaimTypes.Id);

        if (string.IsNullOrEmpty(userId))
            await LogOutAsync(context, ct);

        if (Guid.TryParse(userId, out var id))
        {
            var user = await userRepository.GetByIdAsync(id, ct);

            if (user is null)
                throw new DomainException($"User {id} not found");

            var cookiesRefreshToken = context.Request.Cookies[AuthenticationSchemes.RefreshToken];

            if (string.IsNullOrEmpty(cookiesRefreshToken))
                await LogOutAsync(context, ct);

            var cacheRefreshToken = await redisService.GetStringAsync<string>($"authentication:refresh:{userId}");

            if (cookiesRefreshToken != cacheRefreshToken)
                await LogOutAsync(context, ct);

            await Authentication(context, user);
        }
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

    private void AppendTokenCookie(HttpContext context, string key, string token, int expiresInMinutes)
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

    private async Task Authentication(HttpContext context, User user)
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
            $"authentication:refresh:{user.Id}",
            refreshToken,
            expireTime: TimeSpan.FromMinutes(_authenticationOptions.RefreshTokenExpiresInMinutes));
    }
}