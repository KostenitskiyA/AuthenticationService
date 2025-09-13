using System.Linq.Expressions;

namespace Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task SaveChangesAsync(CancellationToken ct);

    Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression, CancellationToken ct);

    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<T> AddAsync(T entity, CancellationToken ct);

    void Update(T entity);

    void Delete(T entity);
}