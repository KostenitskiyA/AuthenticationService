namespace Application.Interfaces;

public interface IRefreshTokenStorage
{
    Task StoreRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expirationTime, CancellationToken ct = default);

    Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    Task RemoveRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
