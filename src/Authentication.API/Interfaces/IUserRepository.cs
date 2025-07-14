using Authentication.API.Models.Entities;

namespace Authentication.API.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
}