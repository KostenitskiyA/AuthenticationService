using System.Linq.Expressions;
using Authentication.API.Data;
using Authentication.API.Models.Entities;

namespace Authentication.API.Interfaces;

public interface IUserRepository
{
    ApplicationContext Context { get; }

    Task<bool> IsExistsAsync(Expression<Func<User, bool>> expression, CancellationToken ct);

    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<User?> GetByEmailAsync(string email, CancellationToken ct);

    Task<User> AddAsync(User entity, CancellationToken ct);

    Task UpdateAsync(User entity, CancellationToken ct);

    Task DeleteAsync(User entity, CancellationToken ct);
}