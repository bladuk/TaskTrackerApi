using System.Linq.Expressions;

namespace TaskTrackerApi.Repositories.Interfaces;

public interface IRepository<TEntity, in TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}