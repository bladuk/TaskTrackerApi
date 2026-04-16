using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskTrackerApi.DTO.Common;
using TaskTrackerApi.DTO.Projects;
using TaskTrackerApi.Test.Helpers;

namespace TaskTrackerApi.Test.Integration;

[Collection("Integration")]
public class ProjectsControllerTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _seedProjectId;

    public ProjectsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _seedProjectId = factory.SeedProjectId;
    }

    [Fact]
    public async Task GetProjects_ReturnsOkWithPagedResult()
    {
        var response = await _client.GetAsync("/projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>();
        
        body!.Data.Should().NotBeEmpty();
        body.Meta.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(1, 1)]
    public async Task GetProjects_WithPagination_ReturnsCorrectMeta(int page, int pageSize)
    {
        var response = await _client.GetAsync($"/projects?page={page}&pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>();
        
        body!.Meta.Page.Should().Be(page);
        body.Meta.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task GetProjectById_WhenExists_ReturnsOkWithTasks()
    {
        var response = await _client.GetAsync($"/projects/{_seedProjectId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<ProjectWithTasksDto>();
        
        body!.Id.Should().Be(_seedProjectId);
        body.Tasks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProject_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/projects",
            new CreateProjectDto("Unauthorized Project", "Unauthorized Project"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_WithAuth_ReturnsCreated()
    {
        var client = CreateClientWithAuth();
        var dto = new CreateProjectDto("New Integration Project", "New Integration Project");

        var response = await client.PostAsJsonAsync("/projects", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var body = await response.Content.ReadFromJsonAsync<ProjectDto>();
        
        body!.Name.Should().Be(dto.Name);
        body.Description.Should().Be(dto.Description);
    }

    [Fact]
    public async Task CreateProject_WithEmptyName_ReturnsBadRequest()
    {
        var client = CreateClientWithAuth();

        var response = await client.PostAsJsonAsync("/projects",
            new CreateProjectDto("", ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProject_WithAuth_ReturnsOkWithUpdatedData()
    {
        var client = CreateClientWithAuth();
        var dto = new UpdateProjectDto("Updated Name", "Updated Description");

        var response = await client.PutAsJsonAsync($"/projects/{_seedProjectId}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<ProjectDto>();
        
        body!.Name.Should().Be(dto.Name);
        body.Description.Should().Be(dto.Description);
    }

    [Fact]
    public async Task UpdateProject_WhenNotFound_ReturnsNotFound()
    {
        var client = CreateClientWithAuth();

        var response = await client.PutAsJsonAsync($"/projects/{Guid.NewGuid()}",
            new UpdateProjectDto("Name", "Description"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProject_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync($"/projects/{_seedProjectId}",
            new UpdateProjectDto("Name", "Description"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProject_WithAuth_ReturnsNoContent()
    {
        var client = CreateClientWithAuth();
        var createResponse = await client.PostAsJsonAsync("/projects",
            new CreateProjectDto("To Be Deleted", "To Be Deleted"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var response = await client.DeleteAsync($"/projects/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProject_WhenNotFound_ReturnsNotFound()
    {
        var client = CreateClientWithAuth();

        var response = await client.DeleteAsync($"/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProject_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.DeleteAsync($"/projects/{_seedProjectId}");

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