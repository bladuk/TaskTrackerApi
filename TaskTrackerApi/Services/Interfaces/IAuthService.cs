using TaskTrackerApi.DTO.Auth;

namespace TaskTrackerApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto data, CancellationToken ct = default);
    Task<AuthResponseDto> RegisterAsync(RegisterDto data, CancellationToken ct = default);
}