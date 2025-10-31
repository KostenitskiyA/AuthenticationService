using Domain.Entities;

namespace Application.Interfaces;

public interface IGoogleUserRepository : IRepository<GoogleUser>
{
    Task<GoogleUser?> GetByGoogleIdAsync(string googleId, CancellationToken ct);
}
