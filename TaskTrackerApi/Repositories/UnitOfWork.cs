using TaskTrackerApi.Data;
using TaskTrackerApi.Repositories.Interfaces;

namespace TaskTrackerApi.Repositories;

public sealed class UnitOfWork(TaskTrackerDbContext context) : IUnitOfWork
{
    public IProjectRepository Projects => new ProjectRepository(context);
    
    public ITaskRepository Tasks => new TaskRepository(context);
    
    public async Task<int> CommitAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
    
    public void Dispose() => context.Dispose();
}