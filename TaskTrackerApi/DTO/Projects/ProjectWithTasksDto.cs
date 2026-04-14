using TaskTrackerApi.DTO.Tasks;

namespace TaskTrackerApi.DTO.Projects;

public record ProjectWithTasksDto(Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<TaskItemDto> Tasks);