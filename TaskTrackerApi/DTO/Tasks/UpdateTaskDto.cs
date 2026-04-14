namespace TaskTrackerApi.DTO.Tasks;

public record UpdateTaskDto(
    string Title,
    string Description,
    bool IsCompleted,
    Guid ProjectId);