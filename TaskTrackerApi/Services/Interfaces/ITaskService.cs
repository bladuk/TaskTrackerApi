using TaskTrackerApi.DTO.Tasks;

namespace TaskTrackerApi.Services.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItemDto>> GetTasksAsync(bool? isCompleted, Guid? projectId, CancellationToken ct = default);
    Task<TaskItemDto> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskItemDto> CreateTaskAsync(CreateTaskDto data, CancellationToken ct = default);
    Task<TaskItemDto> UpdateTaskAsync(Guid id, UpdateTaskDto data, CancellationToken ct = default);
    Task DeleteTaskAsync(Guid id, CancellationToken ct = default);
}