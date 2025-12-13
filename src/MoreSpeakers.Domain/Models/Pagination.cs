namespace MoreSpeakers.Domain.Models;

public class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public enum SortDirection
{
    Asc = 0,
    Desc = 1
}
