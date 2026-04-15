namespace TaskTrackerApi.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    IUserRepository Users { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
}