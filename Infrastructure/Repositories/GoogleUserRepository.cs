using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GoogleUserRepository(ApplicationContext applicationContext)
    : Repository<GoogleUser>(applicationContext), IGoogleUserRepository
{
    public async Task<GoogleUser?> GetByGoogleIdAsync(string googleId, CancellationToken ct)
    {
        return await DbSet
            .Include(googleUser => googleUser.User)
            .FirstOrDefaultAsync(user => user.GoogleId == googleId, ct);
    }
}