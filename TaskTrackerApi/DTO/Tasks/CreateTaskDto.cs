namespace TaskTrackerApi.DTO.Tasks;

public record CreateTaskDto(
    string Title,
    string Description,
    bool IsCompleted,
    Guid ProjectId);