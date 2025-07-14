using Authentication.API.Data;
using Authentication.API.Interfaces;
using Authentication.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Repositories;

public class UserRepository(ApplicationContext applicationContext)
    : Repository<User>(applicationContext), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await DbSet
            .Include(user => user.GoogleUser)
            .FirstOrDefaultAsync(user => user.Email == email, ct);
    }
}