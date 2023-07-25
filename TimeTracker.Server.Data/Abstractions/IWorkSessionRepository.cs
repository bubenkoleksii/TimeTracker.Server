using TimeTracker.Server.Data.Models.WorkSession;

namespace TimeTracker.Server.Data.Abstractions
{
    public interface IWorkSessionRepository
    {
        public Task<WorkSessionPaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool orderByDesc, 
            int offset, int limit, DateTime? filterDate);
        public Task<WorkSessionDataResponse> GetWorkSessionByIdAsync(Guid id);
        public Task<WorkSessionDataResponse> GetActiveWorkSessionByUserIdAsync(Guid userId);
        public Task<WorkSessionDataResponse> CreateWorkSessionAsync(WorkSessionDataRequest workSession);
        public Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime);
        public Task UpdateWorkSessionAsync(Guid id, WorkSessionDataRequest workSession);
        public Task DeleteWorkSessionAsync(Guid id);
    }
}