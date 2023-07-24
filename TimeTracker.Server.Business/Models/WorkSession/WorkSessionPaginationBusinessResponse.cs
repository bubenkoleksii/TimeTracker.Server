namespace TimeTracker.Server.Business.Models.WorkSession
{
    public class WorkSessionPaginationBusinessResponse<T> where T : class
    {
        public int Count { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}