namespace TaskTrackerApi.DTO.Projects;

public record ProjectDto(Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);