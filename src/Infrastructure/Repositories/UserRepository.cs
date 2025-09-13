using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(ApplicationContext applicationContext)
    : Repository<User>(applicationContext), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await DbSet
            .Include(user => user.GoogleUser)
            .FirstOrDefaultAsync(user => user.Email == email.Trim().ToLowerInvariant(), ct);
    }
}