using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TaskTrackerApi.DTO.Tasks;
using TaskTrackerApi.Exceptions;
using TaskTrackerApi.Models;
using TaskTrackerApi.Repositories.Interfaces;
using TaskTrackerApi.Services.Interfaces;

namespace TaskTrackerApi.Services;

public class TaskService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    ILogger<TaskService> logger) : ITaskService
{
    private const string VersionKey = "tasks:version";
    
    private readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };
    
    public async Task<IReadOnlyList<TaskItemDto>> GetTasksAsync(bool? isCompleted, Guid? projectId, CancellationToken ct = default)
    {
        string version = (string?)await redis.GetDatabase().StringGetAsync(VersionKey) ?? "0";
        string cacheKey = $"tasks:{version}:{isCompleted?.ToString() ?? "any"}:{projectId?.ToString() ?? "any"}";

        string? cached = await cache.GetStringAsync(cacheKey, ct);

        if (cached != null)
            return JsonSerializer.Deserialize<IReadOnlyList<TaskItemDto>>(cached)!;

        List<TaskItem> tasks = await unitOfWork.Tasks.GetFilteredAsync(isCompleted, projectId, ct);
        IReadOnlyList<TaskItemDto> result = mapper.Map<IReadOnlyList<TaskItemDto>>(tasks);

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), _cacheOptions, ct);
        return result;
    }

    public async Task<TaskItemDto> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        TaskItem? task = await unitOfWork.Tasks.GetByIdAsync(id, ct);
        
        return task == null
            ? throw new NotFoundException("Task", id.ToString())
            : mapper.Map<TaskItemDto>(task);
    }

    public async Task<TaskItemDto> CreateTaskAsync(CreateTaskDto data, CancellationToken ct = default)
    {
        if (!await unitOfWork.Projects.AnyAsync(p => p.Id == data.ProjectId, ct))
            throw new NotFoundException("Project", data.ProjectId.ToString());
        
        TaskItem task = new()
        {
            Id = Guid.NewGuid(),
            Title = data.Title,
            Description = data.Description,
            IsCompleted = data.IsCompleted,
            ProjectId = data.ProjectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await unitOfWork.Tasks.AddAsync(task, ct);
        await unitOfWork.CommitAsync(ct);
        await IncrementCacheVersionAsync();
        
        logger.LogInformation("Task created: {TaskId}", task.Id);
        return mapper.Map<TaskItemDto>(task);
    }

    public async Task<TaskItemDto> UpdateTaskAsync(Guid id, UpdateTaskDto data, CancellationToken ct = default)
    {
        TaskItem? task = await unitOfWork.Tasks.GetByIdAsync(id, ct);
        
        if (task == null)
            throw new NotFoundException("Task", id.ToString());
        
        if (task.ProjectId != data.ProjectId && !await unitOfWork.Projects.AnyAsync(p => p.Id == data.ProjectId, ct))
            throw new NotFoundException("Project", data.ProjectId.ToString());

        task.Title = data.Title;
        task.Description = data.Description;
        task.IsCompleted = data.IsCompleted;
        task.ProjectId = data.ProjectId;
        task.UpdatedAt = DateTime.UtcNow;
        
        await unitOfWork.CommitAsync(ct);
        await IncrementCacheVersionAsync();
        
        logger.LogInformation("Task updated: {TaskId}", task.Id);
        return mapper.Map<TaskItemDto>(task);
    }

    public async Task DeleteTaskAsync(Guid id, CancellationToken ct = default)
    {
        TaskItem? task = await unitOfWork.Tasks.GetByIdAsync(id, ct);
        
        if (task == null)
            throw new NotFoundException("Task", id.ToString());
        
        unitOfWork.Tasks.Remove(task);
        await unitOfWork.CommitAsync(ct);
        await IncrementCacheVersionAsync();
        
        logger.LogInformation("Task deleted: {TaskId}", task.Id);
    }
    
    private async Task IncrementCacheVersionAsync()
    {
        await redis.GetDatabase().StringIncrementAsync(VersionKey);
    }
}