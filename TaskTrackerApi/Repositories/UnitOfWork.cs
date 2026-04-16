using TaskTrackerApi.Data;
using TaskTrackerApi.Repositories.Interfaces;

namespace TaskTrackerApi.Repositories;

public sealed class UnitOfWork(TaskTrackerDbContext context) : IUnitOfWork
{
    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;
    private IUserRepository? _users;

    public IProjectRepository Projects => _projects ??= new ProjectRepository(context);

    public ITaskRepository Tasks => _tasks ??= new TaskRepository(context);

    public IUserRepository Users => _users ??= new UserRepository(context);
    
    public async Task<int> CommitAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
    
    public void Dispose() => context.Dispose();
}