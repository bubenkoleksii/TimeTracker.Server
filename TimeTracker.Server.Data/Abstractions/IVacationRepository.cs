using TimeTracker.Server.Data.Models.Vacation;

namespace TimeTracker.Server.Data.Abstractions;

public interface IVacationRepository
{
    public Task<VacationDataResponse> GetVacationByIdAsync(Guid id);

    public Task<VacationDataResponse> GetActiveOrNotRespondedVacationUserIdAsync(Guid userId);

    public Task<IEnumerable<VacationDataResponse>> GetVacationsByUserIdAsync(Guid userId, bool? onlyApproved, bool orderByDesc);

    public Task<IEnumerable<VacationDataResponse>> GetVacationRequestsAsync();

    public Task<IEnumerable<VacationDataResponse>> GetNotStartedUpdatedVacationsAsync();

    public Task<List<VacationDataResponse>> GetNotDeclinedNotFinishedVacationsAsync();

    public Task<VacationDataResponse> CreateVacationAsync(VacationDataRequest vacationDataRequest);

    public Task ApproverUpdateVacationAsync(VacationApproveDataRequest vacationApproveDataRequest);

    public Task DeclineVacationsAsync(List<VacationDataResponse> vacationDataResponses);

    public Task DeleteVacationAsync(Guid id);
}