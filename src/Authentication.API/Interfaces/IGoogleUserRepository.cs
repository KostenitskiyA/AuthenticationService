using Authentication.API.Models.Entities;

namespace Authentication.API.Interfaces;

public interface IGoogleUserRepository : IRepository<GoogleUser>
{
    Task<GoogleUser?> GetByGoogleIdAsync(string googleId, CancellationToken ct);
}