namespace TaskTrackerApi.DTO.Tasks;

public record TaskItemDto(
    Guid Id,
    string Title,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid ProjectId);
