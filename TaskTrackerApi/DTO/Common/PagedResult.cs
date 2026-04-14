namespace TaskTrackerApi.DTO.Common;

public record PagedResult<T>(IReadOnlyList<T> Data, PagedResultMeta Meta);