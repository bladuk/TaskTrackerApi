namespace TaskTrackerApi.Exceptions;

public class AppException(string message, string title, int statusCode = StatusCodes.Status500InternalServerError)
    : Exception(message)
{
    public string Title { get; } = title;

    public int StatusCode { get; } = statusCode;
}