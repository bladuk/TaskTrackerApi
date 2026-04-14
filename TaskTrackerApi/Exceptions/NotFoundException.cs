namespace TaskTrackerApi.Exceptions;

public class NotFoundException(string resource, string id) : AppException($"{resource} with id {id} not found.", "Not Found", StatusCodes.Status404NotFound);