using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using TaskTrackerApi.Repositories.Abstractions;
using TaskTrackerApi.Repositories.Interfaces;

namespace TaskTrackerApi.Repositories;

public class ProjectRepository(TaskTrackerDbContext context) : RepositoryBase<Project, Guid>(context), IProjectRepository
{
    public async Task<Project?> GetByIdWithTasksAsync(Guid id, CancellationToken ct = default) =>
        await Entities.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id, ct);
}