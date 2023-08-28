using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.Data.Abstractions
{
    public interface IWorkSessionRepository
    {
        public Task<PaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc,
            int offset, int limit, DateTime? startDate, DateTime? endDate, bool? showPlanned = false);

        Task<List<WorkSessionDataResponse>> GetOneUserWorkSessionsInRangeAsync(Guid userId, DateTime start, DateTime end, WorkSessionTypeEnum? type = null);

        public Task<List<WorkSessionDataResponse>> GetUserWorkSessionsInRangeAsync(List<Guid> userIds, DateTime start, DateTime end, bool hidePlanned = false);

        public Task<WorkSessionDataResponse> GetWorkSessionByIdAsync(Guid id);

        public Task<WorkSessionDataResponse> GetActiveWorkSessionByUserIdAsync(Guid userId);

        public Task<WorkSessionDataResponse> CreateWorkSessionAsync(WorkSessionDataRequest workSession);

        public Task CreateWorkSessionsAsync(List<WorkSessionDataRequest> workSessionsList);

        public Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime);

        public Task UpdateWorkSessionAsync(Guid id, WorkSessionDataUpdateRequest workSession);

        public Task DeleteWorkSessionAsync(Guid id);

        public Task DeleteWorkSessionsAsync(List<WorkSessionDataResponse> workSessionDataResponses);

        public Task DeleteWorkSessionsInRangeAsync(Guid userId, DateTime start, DateTime end, WorkSessionTypeEnum? type = null);
    }
}