using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.API.Exceptions;
using Authentication.API.Interfaces;
using Authentication.API.Models.Dtos;
using Authentication.API.Models.Entities;
using Authentication.API.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Redis.Interfaces;
using AuthenticationConfiguration = Authentication.API.Models.Options.AuthenticationConfiguration;
using AuthenticationOptions = Authentication.API.Models.Options.AuthenticationOptions;
using ClaimTypes = Authentication.API.Models.Enums.ClaimTypes;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;

namespace Authentication.API.Services;

public class UserService(
    IOptionsMonitor<AuthenticationConfiguration> authenticationConfigurationMonitor,
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    IRedisService redisService
) : IUserService
{
    private readonly AuthenticationOptions _authenticationOptions =
        authenticationConfigurationMonitor.CurrentValue.AuthenticationOptions;

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

    public async Task<User> SignUpAsync(HttpContext context, SignInRequest request, CancellationToken ct)
    {
        var isUserExists = await userRepository.IsExistsAsync(user => user.Email == request.Email, ct);

        if (isUserExists)
            throw new DomainException($"User {request.Email} already exist");

        var user = await userRepository.AddAsync(
            new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = !string.IsNullOrEmpty(request.Password)
                    ? BCrypt.Net.BCrypt.HashPassword(request.Password)
                    : string.Empty
            }, ct);
        await userRepository.Context.SaveChangesAsync(ct);

        await Authentication(context, user);

        return user;
    }

    public async Task<User> GoogleSignUpAsync(HttpContext context, CancellationToken ct)
    {
        var result = await context.AuthenticateAsync(AuthenticationSchemes.Google);

        if (!result.Succeeded || result.Principal is null)
            throw new DomainException("Google authentication failed");

        var claims = result.Principal.Claims.ToList();
        var googleId = claims.FirstOrDefault(claim => claim.Type == SystemClaimTypes.NameIdentifier)?.Value;
        var name = claims.FirstOrDefault(claim => claim.Type == SystemClaimTypes.Name)?.Value;
        var email = claims.FirstOrDefault(claim => claim.Type == SystemClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            throw new DomainException("Google authentication failed");

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
        {
            user = await userRepository.AddAsync(
                new User
                {
                    Name = name,
                    Email = email,
                    HasGoogleAuth = true
                }, ct);

            await googleUserRepository.AddAsync(
                new GoogleUser
                {
                    Id = user.Id,
                    GoogleId = googleId
                }, ct);

            await userRepository.Context.SaveChangesAsync(ct);
        }
        else
        {
            user.GoogleUser = new GoogleUser { Id = user.Id, GoogleId = googleId };
            user.HasGoogleAuth = true;
            await userRepository.Context.SaveChangesAsync(ct);
        }

        await Authentication(context, user);
        context.Response.Cookies.Delete($".AspNetCore.{AuthenticationSchemes.GoogleCookie}");

        return user;
    }

    public async Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null)
            throw new DomainException($"User {request.Email} not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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
            throw new NotAuthorizedException();

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
            throw new EntityNotFoundException(nameof(User), email);

        userRepository.Delete(user);
        if (user.HasGoogleAuth)
            googleUserRepository.Delete(user.GoogleUser);
        await userRepository.Context.SaveChangesAsync(ct);

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
                throw new EntityNotFoundException(nameof(User), id.ToString());

            var cookiesRefreshToken = context.Request.Cookies[AuthenticationSchemes.RefreshToken];

            if (string.IsNullOrEmpty(cookiesRefreshToken))
                await LogOutAsync(context, ct);

            var cacheRefreshToken = await redisService.GetStringAsync<string>($"authentication:refresh:{userId}");

            if (cookiesRefreshToken != cacheRefreshToken)
                await LogOutAsync(context, ct);

            await Authentication(context, user);
        }
    }
}