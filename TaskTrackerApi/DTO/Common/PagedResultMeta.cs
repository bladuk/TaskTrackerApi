namespace TaskTrackerApi.DTO.Common;

public record PagedResultMeta(int Page, int PageSize, int Total)
{
    public int LastPage => (int)Math.Ceiling((double)Total / PageSize);
};