using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerApi.DTO.Common;
using TaskTrackerApi.DTO.Projects;
using TaskTrackerApi.Services.Interfaces;

namespace TaskTrackerApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("[controller]")]
[Produces("application/json")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        return Ok(await projectService.GetProjectsAsync(page, pageSize, ct));
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectWithTasksDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectById([FromRoute] Guid id, CancellationToken ct = default)
    {
        return Ok(await projectService.GetProjectByIdAsync(id, ct));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto data, CancellationToken ct = default)
    {
        return Created("", await projectService.CreateProjectAsync(data, ct));
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProject([FromRoute] Guid id, [FromBody] UpdateProjectDto data, CancellationToken ct = default)
    {
        return Ok(await projectService.UpdateProjectAsync(id, data, ct));
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProject([FromRoute] Guid id, CancellationToken ct = default)
    {
        await projectService.DeleteProjectAsync(id, ct);
        return NoContent();
    }
}