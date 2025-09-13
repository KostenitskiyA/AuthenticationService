using System.Security.Claims;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface ITokenService
{
    ClaimsPrincipal CreateUserClaims(User user);

    string GenerateJwt(User user);

    void AppendTokenCookie(HttpContext context, string key, string token, int expiresInMinutes);

    Task AuthenticationAsync(HttpContext context, User user);
}