using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.ReadModels;

namespace TaskManager.Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskSummaryReadModel> TaskSummaries => Set<TaskSummaryReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public async Task<TEntity?> GetByIdAsync<TEntity>(object[] keyValues,CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return await Set<TEntity>()
            .FindAsync(keyValues,cancellationToken);
    }

    public Task<TEntity?> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return Set<TEntity>().FirstOrDefaultAsync(predicate,cancellationToken);
    }

    public Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return Set<TEntity>().AnyAsync(predicate,cancellationToken);
    }

    void IUnitOfWork.Add<TEntity>(TEntity entity)
     where TEntity : class
    {
        Set<TEntity>().Add(entity);
    }

    void IUnitOfWork.Remove<TEntity>(TEntity entity)
        where TEntity : class
    {
        Set<TEntity>().Remove(entity);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker
                     .Entries<BaseEntity>()
                     .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
