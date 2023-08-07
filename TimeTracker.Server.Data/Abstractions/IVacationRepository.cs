using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.Vacation;

namespace TimeTracker.Server.Data.Abstractions;

public interface IVacationRepository
{
    public Task<VacationDataResponse> GetVacationByIdAsync(Guid id);

    public Task<PaginationDataResponse<VacationDataResponse>> GetVacationsByUserIdAsync(Guid userId, bool? onlyApproved, bool orderByDesc, int offset, int limit);

    public Task<IEnumerable<VacationDataResponse>> GetVacationRequestsAsync();

    public Task<IEnumerable<VacationDataResponse>> GetActiveVacationsAsync();

    public Task<VacationDataResponse> CreateVacationAsync(VacationDataRequest vacationDataRequest);

    public Task ApproverUpdateVacationAsync(VacationApproveDataRequest vacationApproveDataRequest);

    public Task DeleteVacationAsync(Guid id);
}