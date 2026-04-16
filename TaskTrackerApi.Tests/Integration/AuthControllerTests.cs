using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskTrackerApi.DTO.Auth;

namespace TaskTrackerApi.Test.Integration;

[Collection("Integration")]
public class AuthControllerTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        var dto = new RegisterDto("newuser_register", "Password1!");

        var response = await _client.PostAsJsonAsync("/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        body!.Token.Should().NotBeNullOrEmpty();
        body.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflict()
    {
        var dto = new RegisterDto("duplicate_user", "Password1!");
        await _client.PostAsJsonAsync("/auth/register", dto);

        var response = await _client.PostAsJsonAsync("/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithEmptyUsername_ReturnsBadRequest()
    {
        var dto = new RegisterDto("", "Password1!");

        var response = await _client.PostAsJsonAsync("/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var username = "loginuser_valid";
        await _client.PostAsJsonAsync("/auth/register", new RegisterDto(username, "Password1!"));

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginDto(username, "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var username = "loginuser_wrong";
        await _client.PostAsJsonAsync("/auth/register", new RegisterDto(username, "Password1!"));

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginDto(username, "wrongpassword"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/auth/login",
            new LoginDto("nonexistent_user", "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}