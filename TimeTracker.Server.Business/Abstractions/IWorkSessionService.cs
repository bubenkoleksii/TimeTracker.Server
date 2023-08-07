using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.WorkSession;

namespace TimeTracker.Server.Business.Abstractions;

public interface IWorkSessionService
{
    public Task<PaginationBusinessResponse<WorkSessionBusinessResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc, int? offset, 
        int? limit, DateTime? startDate, DateTime? endDate);
    public Task<WorkSessionBusinessResponse> GetWorkSessionByIdAsync(Guid id);
    public Task<WorkSessionBusinessResponse> GetActiveWorkSessionByUserIdAsync(Guid userId);
    public Task<WorkSessionBusinessResponse> CreateWorkSessionAsync(WorkSessionBusinessRequest workSession);
    public Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime);
    public Task UpdateWorkSessionAsync(Guid id, WorkSessionBusinessUpdateRequest workSession);
    public Task DeleteWorkSessionAsync(Guid id);
}