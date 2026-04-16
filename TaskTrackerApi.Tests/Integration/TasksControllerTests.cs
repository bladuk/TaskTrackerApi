using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskTrackerApi.DTO.Tasks;
using TaskTrackerApi.Test.Helpers;

namespace TaskTrackerApi.Test.Integration;

[Collection("Integration")]
public class TasksControllerTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _seedTaskId;
    private readonly Guid _seedProjectId;

    public TasksControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _seedTaskId = factory.SeedTaskId;
        _seedProjectId = factory.SeedProjectId;
    }

    [Fact]
    public async Task GetTasks_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        
        body.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTasks_FilterByProjectId_ReturnsMatchingTasks()
    {
        var response = await _client.GetAsync($"/tasks?projectId={_seedProjectId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        
        body.Should().AllSatisfy(t => t.ProjectId.Should().Be(_seedProjectId));
    }

    [Fact]
    public async Task GetTasks_FilterByIsCompleted_ReturnsMatchingTasks()
    {
        var response = await _client.GetAsync("/tasks?isCompleted=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        
        body.Should().AllSatisfy(t => t.IsCompleted.Should().BeFalse());
    }

    [Fact]
    public async Task GetTaskById_WhenExists_ReturnsOk()
    {
        var response = await _client.GetAsync($"/tasks/{_seedTaskId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        
        body!.Id.Should().Be(_seedTaskId);
    }

    [Fact]
    public async Task GetTaskById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/tasks",
            new CreateTaskDto("Unauthorized Task", "Unauthorized Task", false, _seedProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTask_WithAuth_ReturnsCreated()
    {
        var client = CreateClientWithAuth();
        var dto = new CreateTaskDto("New Integration Task", "New Integration Task", false, _seedProjectId);

        var response = await client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var body = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        
        body!.Title.Should().Be(dto.Title);
        body.ProjectId.Should().Be(dto.ProjectId);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_ReturnsBadRequest()
    {
        var client = CreateClientWithAuth();

        var response = await client.PostAsJsonAsync("/tasks",
            new CreateTaskDto("", "", false, _seedProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_WithAuth_ReturnsOkWithUpdatedData()
    {
        var client = CreateClientWithAuth();
        var dto = new UpdateTaskDto("Updated Title", "Updated Description", true, _seedProjectId);

        var response = await client.PutAsJsonAsync($"/tasks/{_seedTaskId}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        
        body!.Title.Should().Be(dto.Title);
        body.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTask_WhenNotFound_ReturnsNotFound()
    {
        var client = CreateClientWithAuth();

        var response = await client.PutAsJsonAsync($"/tasks/{Guid.NewGuid()}",
            new UpdateTaskDto("Title", "Description", false, _seedProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync($"/tasks/{_seedTaskId}",
            new UpdateTaskDto("Title", "Description", false, _seedProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTask_WithAuth_ReturnsNoContent()
    {
        var client = CreateClientWithAuth();
        var createResponse = await client.PostAsJsonAsync("/tasks",
            new CreateTaskDto("To Be Deleted", "To Be Deleted", false, _seedProjectId));
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();

        var response = await client.DeleteAsync($"/tasks/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_WhenNotFound_ReturnsNotFound()
    {
        var client = CreateClientWithAuth();

        var response = await client.DeleteAsync($"/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.DeleteAsync($"/tasks/{_seedTaskId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private HttpClient CreateClientWithAuth()
    {
        var client = _factory.CreateClient();
        
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtHelper.GenerateToken());
        
        return client;
    }
}