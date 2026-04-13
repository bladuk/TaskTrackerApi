using TaskTrackerApi.Models;

namespace TaskTrackerApi.Repositories.Interfaces;

public interface ITaskRepository : IRepository<TaskItem, Guid>
{
    Task<List<TaskItem>> GetFilteredAsync(bool? isCompleted, Guid? projectId, CancellationToken ct = default);
}