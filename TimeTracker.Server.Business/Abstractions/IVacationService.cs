using TimeTracker.Server.Business.Models.Vacation;

namespace TimeTracker.Server.Business.Abstractions;

public interface IVacationService
{
    public Task<IEnumerable<VacationWithUserBusinessResponse>> GetVacationsByUserIdAsync(Guid userId, bool? onlyApproved, bool orderByDesc);

    public Task<List<VacationWithUserBusinessResponse>> GetUsersVacationsForMonth(List<Guid> userIds, DateTime monthDate);

    public Task<IEnumerable<VacationWithUserBusinessResponse>> GetVacationRequestsAsync(bool getNotStarted);

    public Task<VacationInfoBusinessResponse> GetVacationInfoByUserIdAsync(Guid userId);

    public Task<VacationBusinessResponse> CreateVacationAsync(VacationBusinessRequest vacationBusinessRequest);

    public Task ApproverUpdateVacationAsync(VacationApproveBusinessRequest vacationApproveBusinessRequest);

    public Task DeleteVacationAsync(Guid id);
}