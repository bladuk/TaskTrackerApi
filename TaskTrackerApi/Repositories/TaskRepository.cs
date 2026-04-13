using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using TaskTrackerApi.Repositories.Abstractions;
using TaskTrackerApi.Repositories.Interfaces;

namespace TaskTrackerApi.Repositories;

public class TaskRepository(TaskTrackerDbContext context) : RepositoryBase<TaskItem, Guid>(context), ITaskRepository
{
    public async Task<List<TaskItem>> GetFilteredAsync(bool? isCompleted, Guid? projectId, CancellationToken ct = default)
    {
        IQueryable<TaskItem> queryable = Entities.AsQueryable();

        if (isCompleted.HasValue)
            queryable = queryable.Where(t => t.IsCompleted == isCompleted);

        if (projectId.HasValue)
            queryable = queryable.Where(t => t.ProjectId == projectId);

        return await queryable.ToListAsync(ct);
    }
}