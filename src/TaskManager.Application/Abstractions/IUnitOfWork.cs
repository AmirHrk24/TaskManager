using System.Linq.Expressions;

namespace TaskManager.Application.Abstractions;

public interface IUnitOfWork
{
    Task<TEntity?> GetByIdAsync<TEntity>(object[] keyValues,CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<TEntity?> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default)
        where TEntity : class;

    void Add<TEntity>(TEntity entity)
        where TEntity : class;

    void Remove<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
