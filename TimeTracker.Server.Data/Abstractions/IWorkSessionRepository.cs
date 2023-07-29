using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.WorkSession;

namespace TimeTracker.Server.Data.Abstractions
{
    public interface IWorkSessionRepository
    {
        public Task<PaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserId(Guid userId, bool? orderByDesc, 
            int offset, int limit, DateTime? filterDate);

        public Task<WorkSessionDataResponse> GetWorkSessionById(Guid id);

        public Task<WorkSessionDataResponse> GetActiveWorkSessionByUserId(Guid userId);

        public Task<WorkSessionDataResponse> CreateWorkSession(WorkSessionDataRequest workSession);

        public Task SetWorkSessionEnd(Guid id, DateTime endDateTime);

        public Task UpdateWorkSession(Guid id, WorkSessionDataUpdateRequest workSession);

        public Task DeleteWorkSessionAsync(Guid id);
    }
}