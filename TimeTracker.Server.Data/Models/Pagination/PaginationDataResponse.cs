namespace TimeTracker.Server.Data.Models.Pagination;

public class PaginationDataResponse<T> where T : class
{
    public int Count { get; set; }

    public IEnumerable<T>? Items { get; set; }
}