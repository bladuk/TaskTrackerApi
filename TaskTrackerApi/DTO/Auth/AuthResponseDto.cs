namespace TaskTrackerApi.DTO.Auth;

public record AuthResponseDto(string Token, DateTime ExpiresAt);