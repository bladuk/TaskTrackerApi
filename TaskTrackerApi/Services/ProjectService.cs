using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TaskTrackerApi.DTO.Common;
using TaskTrackerApi.DTO.Projects;
using TaskTrackerApi.Exceptions;
using TaskTrackerApi.Models;
using TaskTrackerApi.Repositories.Interfaces;
using TaskTrackerApi.Services.Interfaces;

namespace TaskTrackerApi.Services;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache cache, IConnectionMultiplexer redis, ILogger<ProjectService> logger) : IProjectService
{
    private const string VersionKey = "projects:version";

    private readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        string version = (string?)await redis.GetDatabase().StringGetAsync(VersionKey) ?? "0";
        string cacheKey = $"projects:{version}:{page}:{pageSize}";

        string? cached = await cache.GetStringAsync(cacheKey, ct);

        if (cached != null)
            return JsonSerializer.Deserialize<PagedResult<ProjectDto>>(cached)!;

        var (items, total) = await unitOfWork.Projects.GetPagedAsync(page, pageSize, ct);

        PagedResult<ProjectDto> result = new(
            Data: mapper.Map<IEnumerable<ProjectDto>>(items),
            Meta: new(page, pageSize, total)
        );

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), _cacheOptions, ct);
        return result;
    }

    public async Task<ProjectWithTasksDto> GetProjectByIdAsync(Guid id, CancellationToken ct = default)
    { 
        Project? project = await unitOfWork.Projects.GetByIdWithTasksAsync(id, ct);

        return project == null
            ? throw new NotFoundException("Project", id.ToString())
            : mapper.Map<ProjectWithTasksDto>(project);
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto data, CancellationToken ct = default)
    {
        Project project = new()
        {
            Id = Guid.NewGuid(),
            Name = data.Name,
            Description = data.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await unitOfWork.Projects.AddAsync(project, ct);
        await unitOfWork.CommitAsync(ct);
        await IncrementCacheVersionAsync();

        logger.LogInformation($"Project created: {project.Id}");
        return mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto data, CancellationToken ct = default)
    {
        Project? project = await unitOfWork.Projects.GetByIdAsync(id, ct);
        
        if (project == null)
            throw new NotFoundException("Project", id.ToString());
        
        project.Name = data.Name;
        project.Description = data.Description;
        project.UpdatedAt = DateTime.UtcNow;
        
        await unitOfWork.CommitAsync(ct);
        await IncrementCacheVersionAsync();
        
        logger.LogInformation($"Project updated: {project.Id}");
        return mapper.Map<ProjectDto>(project);
    }

    public async Task DeleteProjectAsync(Guid id, CancellationToken ct = default)
    {
        Project? project = await unitOfWork.Projects.GetByIdAsync(id, ct);
        
        if (project == null)
            throw new NotFoundException("Project", id.ToString());
        
        unitOfWork.Projects.Remove(project);
        await unitOfWork.CommitAsync(ct);
        await IncrementCacheVersionAsync();
        
        logger.LogInformation($"Project deleted: {project.Id}");
    }

    private async Task IncrementCacheVersionAsync()
    {
        await redis.GetDatabase().StringIncrementAsync(VersionKey);
    }
}