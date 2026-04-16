using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskTrackerApi.Controllers;
using TaskTrackerApi.DTO.Projects;
using TaskTrackerApi.DTO.Tasks;
using TaskTrackerApi.Exceptions;
using TaskTrackerApi.Services.Interfaces;
using TaskTrackerApi.Test.Helpers;

namespace TaskTrackerApi.Test.Unit.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _mockService;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockService = new Mock<ITaskService>();
        _controller = new TasksController(_mockService.Object);
    }

    [Fact]
    public async Task GetTasks_NoFilters_ReturnsOkWithTasks()
    {
        var expected = new List<TaskItemDto> { MockHelpers.SampleTaskDto(), MockHelpers.SampleTaskDto() };
        _mockService.Setup(s => s.GetTasksAsync(null, null, default)).ReturnsAsync(expected);

        var result = await _controller.GetTasks(null, null);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.GetTasksAsync(null, null, default), Times.Once);
    }

    [Fact]
    public async Task GetTasks_FilteredByCompletion_ReturnsOkWithTasks()
    {
        var expected = new List<TaskItemDto> { MockHelpers.SampleTaskDto(isCompleted: true) };
        _mockService.Setup(s => s.GetTasksAsync(true, null, default)).ReturnsAsync(expected);

        var result = await _controller.GetTasks(true, null);

        var value = result.Should().BeOfType<OkObjectResult>().Which.Value
            .Should().BeAssignableTo<IReadOnlyList<TaskItemDto>>().Subject;

        value.Should().OnlyContain(t => t.IsCompleted == true);
        value.Should().HaveCount(1);
        _mockService.Verify(s => s.GetTasksAsync(true, null, default), Times.Once);
    }

    [Fact]
    public async Task GetTasks_FilteredByProject_ReturnsOkWithTasks()
    {
        var projectId = Guid.NewGuid();
        var expected = new List<TaskItemDto> { MockHelpers.SampleTaskDto(projectId: projectId) };
        _mockService.Setup(s => s.GetTasksAsync(null, projectId, default)).ReturnsAsync(expected);
        
        var result = await _controller.GetTasks(null, projectId);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.GetTasksAsync(null, projectId, default), Times.Once);
    }

    [Fact]
    public async Task GetTask_WhenExists_ReturnsOk()
    {
        var taskId = Guid.NewGuid();
        var expected = MockHelpers.SampleTaskDto(taskId);
        _mockService.Setup(s => s.GetTaskByIdAsync(taskId, default)).ReturnsAsync(expected);
        
        var result = await _controller.GetTaskById(taskId);
        
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.GetTaskByIdAsync(taskId, default), Times.Once);   
    }

    [Fact]
    public async Task GetTask_WhenNotFound_ThrowsNotFound()
    {
        var taskId = Guid.NewGuid();
        _mockService.Setup(s => s.GetTaskByIdAsync(taskId, default))
            .ThrowsAsync(new NotFoundException(nameof(TaskItemDto), taskId.ToString()));
        
        var act = async () => await _controller.GetTaskById(taskId);
        
        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.GetTaskByIdAsync(taskId, default), Times.Once);  
    }

    [Fact]
    public async Task CreateTask_ReturnsCreated()
    {
        var projectId = Guid.NewGuid();
        var dto = MockHelpers.SampleCreateTaskDto(projectId);
        var expected = MockHelpers.SampleTaskDto(projectId);
        _mockService.Setup(s => s.CreateTaskAsync(dto, default)).ReturnsAsync(expected);
        
        var result = await _controller.CreateTask(dto);
        
        result.Should().BeOfType<CreatedResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.CreateTaskAsync(dto, default), Times.Once); 
    }
    
    [Fact]
    public async Task CreateTask_WhenProjectIdNotFound_ThrowsNotFound()
    {
        var projectId = Guid.NewGuid();
        var dto = MockHelpers.SampleCreateTaskDto(projectId);
        _mockService.Setup(s => s.CreateTaskAsync(dto, default))
            .ThrowsAsync(new NotFoundException(nameof(ProjectDto), projectId.ToString()));
        
        var act = async () => await _controller.CreateTask(dto);
        
        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.CreateTaskAsync(dto, default), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WhenExists_ReturnsOk()
    {
        var taskId = Guid.NewGuid();
        var dto = MockHelpers.SampleUpdateTaskDto();
        var expected = MockHelpers.SampleTaskDto(taskId, title: dto.Title, description: dto.Description, isCompleted: dto.IsCompleted);
        _mockService.Setup(s => s.UpdateTaskAsync(taskId, dto, default)).ReturnsAsync(expected);
        
        var result = await _controller.UpdateTask(taskId, dto);
        
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.UpdateTaskAsync(taskId, dto, default), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WhenNotFound_ThrowsNotFound()
    {
        var taskId = Guid.NewGuid();
        var dto = MockHelpers.SampleUpdateTaskDto();
        _mockService.Setup(s => s.UpdateTaskAsync(taskId, dto, default))
            .ThrowsAsync(new NotFoundException(nameof(TaskItemDto), taskId.ToString()));
        
        var act = async () => await _controller.UpdateTask(taskId, dto);
        
        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.UpdateTaskAsync(taskId, dto, default), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WhenProjectNotFound_ThrowsNotFound()
    {
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = MockHelpers.SampleUpdateTaskDto(projectId);
        _mockService.Setup(s => s.UpdateTaskAsync(taskId, dto, default))
            .ThrowsAsync(new NotFoundException(nameof(ProjectDto), projectId.ToString()));
        
        var act = async () => await _controller.UpdateTask(taskId, dto);
        
        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.UpdateTaskAsync(taskId, dto, default), Times.Once);   
    }
    
    [Fact]
    public async Task DeleteTask_WhenExists_ReturnsNoContent()
    {
        var taskId = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteTaskAsync(taskId, default)).Returns(Task.CompletedTask);
        
        var result = await _controller.DeleteTask(taskId);
        
        result.Should().BeOfType<NoContentResult>();
        _mockService.Verify(s => s.DeleteTaskAsync(taskId, default), Times.Once);
    }
    
    [Fact]
    public async Task DeleteTask_WhenNotFound_ThrowsNotFound()
    {
        var taskId = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteTaskAsync(taskId, default))
            .ThrowsAsync(new NotFoundException(nameof(TaskItemDto), taskId.ToString()));
        
        var act = async () => await _controller.DeleteTask(taskId);
        
        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.DeleteTaskAsync(taskId, default), Times.Once);
    }
}