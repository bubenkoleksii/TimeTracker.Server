using TimeTracker.Server.Data.Models.SickLeave;

namespace TimeTracker.Server.Data.Abstractions;

public interface ISickLeaveRepository
{
    public Task<SickLeaveDataResponse> GetSickLeaveByIdAsync(Guid id);

    public Task<List<SickLeaveDataResponse>> GetSickLeavesAsync(DateTime date, Guid? userId, bool searchByYear = false);

    public Task<SickLeaveDataResponse> CreateSickLeaveAsync(SickLeaveDataRequest sickLeaveDataRequest);

    public Task UpdateSickLeaveAsync(Guid id, SickLeaveDataRequest sickLeaveDataRequest);

    public Task DeleteSickLeaveAsync(Guid id);
}