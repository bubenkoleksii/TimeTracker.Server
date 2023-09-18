using TimeTracker.Server.Business.Models.Vacation;

namespace TimeTracker.Server.Business.Abstractions;

public interface IVacationService
{
    public Task<IEnumerable<VacationBusinessResponse>> GetVacationsByUserIdAsync(Guid userId, bool? onlyApproved, bool orderByDesc);

    public Task<List<VacationBusinessResponse>> GetUsersVacationsForMonth(List<Guid> userIds, DateTime monthDate);

    public Task<IEnumerable<VacationBusinessResponse>> GetVacationRequestsAsync(bool getNotStarted);

    public Task<VacationInfoBusinessResponse> GetVacationInfoByUserIdAsync(Guid userId);

    public Task<VacationBusinessResponse> CreateVacationAsync(VacationBusinessRequest vacationBusinessRequest);

    public Task ApproverUpdateVacationAsync(VacationApproveBusinessRequest vacationApproveBusinessRequest);

    public Task DeleteVacationAsync(Guid id);
}