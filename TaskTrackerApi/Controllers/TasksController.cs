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
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] bool? isCompleted, [FromQuery] Guid? projectId, CancellationToken ct = default)
    {
        return Ok(await taskService.GetTasksAsync(isCompleted, projectId, ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskById([FromRoute] Guid id, CancellationToken ct = default)
    {
        return Ok(await taskService.GetTaskByIdAsync(id, ct));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto data, CancellationToken ct = default)
    {
        return Created("", await taskService.CreateTaskAsync(data, ct));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTask([FromRoute] Guid id, [FromBody] UpdateTaskDto data, CancellationToken ct = default)
    {
        return Ok(await taskService.UpdateTaskAsync(id, data, ct));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid id, CancellationToken ct = default)
    {
        await taskService.DeleteTaskAsync(id, ct);
        return NoContent();
    }
}