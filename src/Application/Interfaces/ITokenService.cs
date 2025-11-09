using Domain.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    Task AppendTokensAsync(User user);

    Task RevokeTokensAsync();

    Task RefreshTokensAsync(CancellationToken ct);
}
