namespace TaskTrackerApi.DTO.Common;

public record PagedResult<T>(IEnumerable<T> Data, PagedResultMeta Meta);