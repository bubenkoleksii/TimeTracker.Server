namespace TimeTracker.Server.Data.Models.WorkSession
{
    public class WorkSessionPaginationDataResponse<T> where T : class
    {
        public int Count { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}