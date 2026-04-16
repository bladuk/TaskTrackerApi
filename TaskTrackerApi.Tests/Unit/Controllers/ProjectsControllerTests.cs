using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskTrackerApi.Controllers;
using TaskTrackerApi.DTO.Projects;
using TaskTrackerApi.Exceptions;
using TaskTrackerApi.Services.Interfaces;
using TaskTrackerApi.Test.Helpers;

namespace TaskTrackerApi.Test.Unit.Controllers;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> _mockService;
    private readonly ProjectsController _controller;

    public ProjectsControllerTests()
    {
        _mockService = new Mock<IProjectService>();
        _controller = new ProjectsController(_mockService.Object);
    }

    [Fact]
    public async Task GetProjects_ReturnsPagedResult()
    {
        var expected = MockHelpers.SamplePagedProjectsDto();
        _mockService.Setup(s => s.GetProjectsAsync(1, 10, default)).ReturnsAsync(expected);

        var result = await _controller.GetProjects(1, 10);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.GetProjectsAsync(1, 10, default), Times.Once);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(3, 30)]
    public async Task GetProjects_PassCustomParams(int page, int pageSize)
    {
        var expected = MockHelpers.SamplePagedProjectsDto(2);
        _mockService.Setup(s => s.GetProjectsAsync(page, pageSize, default)).ReturnsAsync(expected);

        var result = await _controller.GetProjects(page, pageSize);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.GetProjectsAsync(page, pageSize, default), Times.Once);
    }

    [Fact]
    public async Task GetProject_WhenExists_ReturnsProject()
    {
        var projectId = Guid.NewGuid();
        var expected = MockHelpers.SampleProjectWithTasksDto(projectId);
        _mockService.Setup(s => s.GetProjectByIdAsync(projectId, default)).ReturnsAsync(expected);
        
        var result = await _controller.GetProjectById(projectId);
        
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.GetProjectByIdAsync(projectId, default), Times.Once);
    }

    [Fact]
    public async Task GetProject_WhenNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetProjectByIdAsync(id, default))
            .ThrowsAsync(new NotFoundException(nameof(ProjectDto), id.ToString()));

        var act = async () => await _controller.GetProjectById(id);

        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.GetProjectByIdAsync(id, default), Times.Once);
    }

    [Fact]
    public async Task CreateProject_ReturnsCreated()
    {
        var dto = MockHelpers.SampleCreateProjectDto();
        var expected = MockHelpers.SampleProjectDto();
        _mockService.Setup(s => s.CreateProjectAsync(dto, default)).ReturnsAsync(expected);
        
        var result = await _controller.CreateProject(dto);
        
        result.Should().BeOfType<CreatedResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.CreateProjectAsync(dto, default), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_WhenExists_ReturnsUpdated()
    {
        var projectId = Guid.NewGuid();
        var dto = MockHelpers.SampleUpdateProjectDto();
        var expected = MockHelpers.SampleProjectDto(name: dto.Name, description: dto.Description);

        _mockService.Setup(s => s.UpdateProjectAsync(projectId, dto, default)).ReturnsAsync(expected);

        var result = await _controller.UpdateProject(projectId, dto);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        _mockService.Verify(s => s.UpdateProjectAsync(projectId, dto, default), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_WhenNotFound_ThrowsNotFound()
    {
        var projectId = Guid.NewGuid();
        var dto = MockHelpers.SampleUpdateProjectDto();
        _mockService.Setup(s => s.UpdateProjectAsync(projectId, dto, default))
            .ThrowsAsync(new NotFoundException(nameof(ProjectDto), projectId.ToString()));

        var act = async () => await _controller.UpdateProject(projectId, dto);

        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.UpdateProjectAsync(projectId, dto, default), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_WhenExists_ReturnsNoContent()
    {
        var projectId = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteProjectAsync(projectId, default)).Returns(Task.CompletedTask);
        
        var result = await _controller.DeleteProject(projectId);
        
        result.Should().BeOfType<NoContentResult>();
        _mockService.Verify(s => s.DeleteProjectAsync(projectId, default), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_WhenNotFound_ThrowsNotFound()
    {
        var projectId = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteProjectAsync(projectId, default))
            .ThrowsAsync(new NotFoundException(nameof(ProjectDto), projectId.ToString()));
        
        var act = async () => await _controller.DeleteProject(projectId);
        
        await act.Should().ThrowAsync<NotFoundException>();
        _mockService.Verify(s => s.DeleteProjectAsync(projectId, default), Times.Once);
    }
}