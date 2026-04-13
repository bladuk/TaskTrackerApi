using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Repositories.Interfaces;

namespace TaskTrackerApi.Repositories.Abstractions;

public class RepositoryBase<TEntity, TKey>(TaskTrackerDbContext context) : IRepository<TEntity, TKey> where TEntity : class
{
    protected readonly DbSet<TEntity> Entities = context.Set<TEntity>();
    
    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        await Entities.FindAsync([id], ct);

    public async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await Entities.ToListAsync(ct);

    public async Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        int totalCount = await Entities.CountAsync(ct);
        List<TEntity> items = await Entities
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await Entities.AddAsync(entity, ct);

    public void Update(TEntity entity) =>
        Entities.Update(entity);

    public void Remove(TEntity entity) =>
        Entities.Remove(entity);
}