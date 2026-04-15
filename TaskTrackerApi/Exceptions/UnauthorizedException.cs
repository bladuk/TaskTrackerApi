namespace TaskTrackerApi.Exceptions;

public class UnauthorizedException(string message) : AppException(message, "Unauthorized", StatusCodes.Status401Unauthorized);