using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerApi.DTO.Tasks;
using TaskTrackerApi.Services.Interfaces;

namespace TaskTrackerApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("[controller]")]
[Produces("application/json")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    /// <summary>Get all tasks, optionally filtered by completion status or project</summary>
    /// <param name="isCompleted">Filter by completion status</param>
    /// <param name="projectId">Filter by project GUID</param>
    /// <response code="200">Returns the list of matching tasks</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] bool? isCompleted, [FromQuery] Guid? projectId, CancellationToken ct = default)
    {
        return Ok(await taskService.GetTasksAsync(isCompleted, projectId, ct));
    }

    /// <summary>Get a single task by ID</summary>
    /// <param name="id">Task GUID</param>
    /// <response code="200">Returns the task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById([FromRoute] Guid id, CancellationToken ct = default)
    {
        return Ok(await taskService.GetTaskByIdAsync(id, ct));
    }

    /// <summary>Create a new task</summary>
    /// <param name="data">Task title, description, completion status, and parent project ID</param>
    /// <response code="201">Task created successfully. Returns created task</response>
    /// <response code="400">Validation failed (title is empty or exceeds 255 characters)</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">Specified project not found</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto data, CancellationToken ct = default)
    {
        return Created("", await taskService.CreateTaskAsync(data, ct));
    }

    /// <summary>Update an existing task</summary>
    /// <param name="id">Task GUID</param>
    /// <param name="data">Updated title, description, completion status, and project ID</param>
    /// <response code="200">Task updated successfully. Returns updated task</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">Task or target project not found</response>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask([FromRoute] Guid id, [FromBody] UpdateTaskDto data, CancellationToken ct = default)
    {
        return Ok(await taskService.UpdateTaskAsync(id, data, ct));
    }

    /// <summary>Delete a task by ID</summary>
    /// <param name="id">Task GUID</param>
    /// <response code="204">Task deleted successfully</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">Task not found</response>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid id, CancellationToken ct = default)
    {
        await taskService.DeleteTaskAsync(id, ct);
        return NoContent();
    }
}