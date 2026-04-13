using TaskTrackerApi.Models;

namespace TaskTrackerApi.Repositories.Interfaces;

public interface IProjectRepository : IRepository<Project, Guid>
{
    Task<Project?> GetByIdWithTasksAsync(Guid id, CancellationToken ct = default);
}