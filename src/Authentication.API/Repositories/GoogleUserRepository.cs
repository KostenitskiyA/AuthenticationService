using Authentication.API.Data;
using Authentication.API.Interfaces;
using Authentication.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Repositories;

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