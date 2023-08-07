using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.WorkSession;

namespace TimeTracker.Server.Data.Abstractions
{
    public interface IWorkSessionRepository
    {
        public Task<PaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc, 
            int offset, int limit, DateTime? startDate, DateTime? endDate);

        public Task<WorkSessionDataResponse> GetWorkSessionByIdAsync(Guid id);

        public Task<WorkSessionDataResponse> GetActiveWorkSessionByUserIdAsync(Guid userId);

        public Task<WorkSessionDataResponse> CreateWorkSessionAsync(WorkSessionDataRequest workSession);

        public Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime);

        public Task UpdateWorkSessionAsync(Guid id, WorkSessionDataUpdateRequest workSession);

        public Task DeleteWorkSessionAsync(Guid id);
    }
}