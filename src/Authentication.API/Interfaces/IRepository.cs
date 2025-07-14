using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Interfaces;

public interface IRepository<T> where T : class
{
    DbContext Context { get; }

    Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression, CancellationToken ct);

    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<T> AddAsync(T entity, CancellationToken ct);

    void Update(T entity);

    void Delete(T entity);
}
