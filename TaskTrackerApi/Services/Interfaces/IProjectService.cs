using TaskTrackerApi.DTO.Common;
using TaskTrackerApi.DTO.Projects;

namespace TaskTrackerApi.Services.Interfaces;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ProjectWithTasksDto> GetProjectByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto data, CancellationToken ct = default);
    Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto data, CancellationToken ct = default);
    Task DeleteProjectAsync(Guid id, CancellationToken ct = default);
}