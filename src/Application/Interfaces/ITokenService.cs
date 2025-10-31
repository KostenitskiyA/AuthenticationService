using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface ITokenService
{
    Task AppendTokensAsync(HttpContext context, User user);

    Task RevokeTokensAsync(HttpContext context);

    Task RefreshTokensAsync(HttpContext context, CancellationToken ct);
}
