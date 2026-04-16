using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
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
    /// <summary>Get a paginated list of all projects</summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <response code="200">Returns the requested page of projects with pagination metadata</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        return Ok(await projectService.GetProjectsAsync(page, pageSize, ct));
    }

    /// <summary>Get a single project by ID including its tasks</summary>
    /// <param name="id">Project GUID</param>
    /// <response code="200">Returns the project with its associated tasks</response>
    /// <response code="404">Project not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectWithTasksDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById([FromRoute] Guid id, CancellationToken ct = default)
    {
        return Ok(await projectService.GetProjectByIdAsync(id, ct));
    }

    /// <summary>Create a new project</summary>
    /// <param name="data">Project name and description</param>
    /// <response code="201">Project created successfully. Returns created project</response>
    /// <response code="400">Validation failed (name is empty or exceeds 255 characters)</response>
    /// <response code="401">Authentication required</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto data, CancellationToken ct = default)
    {
        return Created("", await projectService.CreateProjectAsync(data, ct));
    }

    /// <summary>Update an existing project</summary>
    /// <param name="id">Project GUID</param>
    /// <param name="data">Updated name and description</param>
    /// <response code="200">Project updated successfully. Returns updated project</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">Project not found</response>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject([FromRoute] Guid id, [FromBody] UpdateProjectDto data, CancellationToken ct = default)
    {
        return Ok(await projectService.UpdateProjectAsync(id, data, ct));
    }

    /// <summary>Delete a project by ID</summary>
    /// <param name="id">Project GUID</param>
    /// <response code="204">Project deleted successfully</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">Project not found</response>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject([FromRoute] Guid id, CancellationToken ct = default)
    {
        await projectService.DeleteProjectAsync(id, ct);
        return NoContent();
    }
}