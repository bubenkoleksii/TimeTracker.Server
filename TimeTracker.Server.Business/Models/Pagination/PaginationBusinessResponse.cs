namespace TimeTracker.Server.Business.Models.Pagination;

public record PaginationBusinessResponse<T> where T : class
{
    public int Count { get; set; }

    public IEnumerable<T>? Items { get; set; }
}