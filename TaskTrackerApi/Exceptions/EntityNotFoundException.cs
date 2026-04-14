namespace TaskTrackerApi.Exceptions;

public class EntityNotFoundException(string entity, string id) : Exception($"{entity} with ID {id} not found.");