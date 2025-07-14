using System.Linq.Expressions;
using Authentication.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Repositories;

public class Repository<T>(DbContext context) : IRepository<T> where T : class
{
    public DbContext Context => context;
    
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression, CancellationToken ct)
    {
        return await DbSet.AnyAsync(expression, ct);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await DbSet.FindAsync([id], ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct)
    {
        var entry = await DbSet.AddAsync(entity, ct);
        return entry.Entity;
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        DbSet.Remove(entity);
    }
}