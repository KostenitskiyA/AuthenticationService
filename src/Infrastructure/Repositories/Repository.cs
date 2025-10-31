using System.Linq.Expressions;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class Repository<T>(DbContext context) : IRepository<T> where T : class
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task SaveChangesAsync(CancellationToken ct) => await context.SaveChangesAsync(ct);

    public virtual async Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression, CancellationToken ct) =>
        await DbSet.AnyAsync(expression, ct);

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct) => await DbSet.FindAsync([id], ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct) => (await DbSet.AddAsync(entity, ct)).Entity;

    public virtual void Update(T entity) => DbSet.Update(entity);

    public virtual void Delete(T entity) => DbSet.Remove(entity);
}
