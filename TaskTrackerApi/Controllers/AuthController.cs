using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerApi.DTO.Auth;
using TaskTrackerApi.Services.Interfaces;

namespace TaskTrackerApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Authenticate an existing user and return a JWT token</summary>
    /// <param name="loginDto">Username and password</param>
    /// <response code="200">Authentication successful. Returns a JWT token and its expiry</response>
    /// <response code="400">Validation failed (empty username or password)</response>
    /// <response code="401">Invalid username or password</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken ct = default)
    {
        return Ok(await authService.LoginAsync(loginDto, ct));
    }

    /// <summary>Register a new user and return a JWT token</summary>
    /// <param name="registerDto">Desired username and password</param>
    /// <response code="200">Registration successful. Returns a JWT token and its expiry</response>
    /// <response code="400">Validation failed (password too short, too long or missing special char)</response>
    /// <response code="409">Username is already taken</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken ct = default)
    {
        return Ok(await authService.RegisterAsync(registerDto, ct));
    }
}