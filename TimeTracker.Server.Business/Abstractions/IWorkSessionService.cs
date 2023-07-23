using TimeTracker.Server.Business.Models.WorkSession;

namespace TimeTracker.Server.Business.Abstractions
{
    public interface IWorkSessionService
    {
        public Task<WorkSessionPaginationBusinessResponse<WorkSessionBusinessResponse>> GetWorkSessionsByUserId(Guid userId, bool orderByDesc, int offset, 
            int limit, DateTime? filterDate);
        public Task<WorkSessionBusinessResponse> GetWorkSessionById(Guid id);
        public Task<WorkSessionBusinessResponse> GetActiveWorkSessionByUserId(Guid userId);
        public Task<WorkSessionBusinessResponse> CreateWorkSessionAsync(WorkSessionBusinessRequest workSession);
        public Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime);
    }
}