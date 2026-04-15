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
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken ct = default)
    {
        return Ok(await authService.LoginAsync(loginDto, ct));
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken ct = default)
    {
        return Ok(await authService.RegisterAsync(registerDto, ct));
    }
}