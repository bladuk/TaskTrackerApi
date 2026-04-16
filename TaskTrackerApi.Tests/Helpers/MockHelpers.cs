using TaskTrackerApi.DTO.Common;
using TaskTrackerApi.DTO.Projects;
using TaskTrackerApi.DTO.Tasks;

namespace TaskTrackerApi.Test.Helpers;

public static class MockHelpers
{
    public static ProjectDto SampleProjectDto(Guid? id = null, string name = "Sample Project", string description = "Sample Description") =>
        new(id ?? Guid.NewGuid(), name, description, DateTime.UtcNow, DateTime.UtcNow);

    public static PagedResult<ProjectDto> SamplePagedProjectsDto(int count = 3) => new(
        Data: Enumerable.Range(0, count).Select(i => SampleProjectDto()).ToList(),
        Meta: new PagedResultMeta(1, 10, count)
    );
    
    public static TaskItemDto SampleTaskDto(Guid? id = null, Guid? projectId = null) =>
        new(id ?? Guid.NewGuid(), "Sample Task", "Sample Description", false, DateTime.UtcNow, DateTime.UtcNow, projectId ?? Guid.NewGuid());

    public static ProjectWithTasksDto SampleProjectWithTasksDto(Guid? id = null)
    {
        Guid projectId = id ?? Guid.NewGuid();
        return new(projectId, "Sample Project", "Sample Description", DateTime.UtcNow, DateTime.UtcNow,
        [
            SampleTaskDto(Guid.NewGuid(), projectId)
        ]);
    }
    
    public static CreateProjectDto SampleCreateProjectDto() => 
        new("Sample Project", "Sample Description");
    
    public static UpdateProjectDto SampleUpdateProjectDto() =>
        new("Updated Name", "Updated Description");
}