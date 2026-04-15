namespace TaskTrackerApi.Exceptions;

public class ConflictException(string message) : AppException(message, "Conflict", StatusCodes.Status409Conflict);