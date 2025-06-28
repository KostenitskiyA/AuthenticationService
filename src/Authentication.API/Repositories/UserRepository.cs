using System.Linq.Expressions;
using Authentication.API.Data;
using Authentication.API.Exceptions;
using Authentication.API.Interfaces;
using Authentication.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Repositories;

public class UserRepository(ApplicationContext dbContext) : IUserRepository
{
    public ApplicationContext Context => dbContext;

    public async Task<bool> IsExistsAsync(Expression<Func<User, bool>> expression, CancellationToken ct)
    {
        var isUserExists = await dbContext.Users.AnyAsync(expression, ct);

        return isUserExists;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, ct);

        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, ct);

        return user;
    }

    public async Task<User> AddAsync(User entity, CancellationToken ct)
    {
        var user = await dbContext.Users.AddAsync(entity, ct);
        await dbContext.SaveChangesAsync(ct);

        return user.Entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken ct)
    {
        var isUserExists = await dbContext.Users.AnyAsync(user => user.Id == entity.Id, ct);

        if (!isUserExists)
            throw new DomainException($"User {entity.Email} not found");

        dbContext.Users.Update(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(User entity, CancellationToken ct)
    {
        dbContext.Users.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
    }
}