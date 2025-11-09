using Application.Interfaces;
using Redis.Interfaces;

namespace Infrastructure.Services;

public class RedisRefreshTokenStorage(IRedisService redisService) : IRefreshTokenStorage
{
    private static string GetKey(string refreshToken) => $"authentication:refresh:{refreshToken}";

    public async Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var key = GetKey(refreshToken);
        var value = await redisService.GetStringAsync<string>(key);

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            return null;

        return userId;
    }

    public async Task SaveRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expirationTime)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var key = GetKey(refreshToken);
        await redisService.SetStringAsync(key, userId.ToString(), expireTime: expirationTime);
    }

    public async Task RemoveRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var key = GetKey(refreshToken);
        await redisService.DeleteKeyAsync(key);
    }
}
