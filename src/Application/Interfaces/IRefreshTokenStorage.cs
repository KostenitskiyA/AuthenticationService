namespace Application.Interfaces;

public interface IRefreshTokenStorage
{
    Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken);

    Task SaveRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expirationTime);

    Task RemoveRefreshTokenAsync(string refreshToken);
}
