using Application.Interfaces;
using Redis.Interfaces;

namespace Infrastructure.Services;

public class RedisRefreshTokenStorage(IRedisService redisService) : IRefreshTokenStorage
{
    private const string KeyPrefix = "authentication:refresh:";

    public async Task StoreRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expirationTime, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty", nameof(refreshToken));

        var key = GetKey(refreshToken);
        await redisService.SetStringAsync(key, userId.ToString(), expireTime: expirationTime);
    }

    public async Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var key = GetKey(refreshToken);
        var userIdString = await redisService.GetStringAsync<string>(key);

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return null;

        return userId;
    }

    public async Task RemoveRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var key = GetKey(refreshToken);
        await redisService.DeleteKeyAsync(key);
    }

    private static string GetKey(string refreshToken) => $"{KeyPrefix}{refreshToken}";
}

