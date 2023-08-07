using TimeTracker.Server.Data.Models.Vacation;

namespace TimeTracker.Server.Data.Abstractions;

public interface IVacationInfoRepository
{
    public Task<VacationInfoDataResponse> GetVacationInfoByUserIdAsync(Guid userId);

    public Task<VacationInfoDataResponse> CreateVacationInfoAsync(Guid userId);

    public Task AddDaysSpentAsync(Guid userId, int daysSpent);
}