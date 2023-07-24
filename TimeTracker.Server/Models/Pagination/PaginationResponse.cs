namespace TimeTracker.Server.Models.Pagination;

public record PaginationResponse<T> where T : class
{
    public int Count { get; set; }

    public IEnumerable<T>? Items { get; set; }
}