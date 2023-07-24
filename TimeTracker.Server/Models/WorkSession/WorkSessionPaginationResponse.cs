namespace TimeTracker.Server.Models.WorkSession
{
    public class WorkSessionPaginationResponse<T> where T : class
    {
        public int Count { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}