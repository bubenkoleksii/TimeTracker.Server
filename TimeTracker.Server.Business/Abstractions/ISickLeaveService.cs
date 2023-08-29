using TimeTracker.Server.Business.Models.SickLeave;

namespace TimeTracker.Server.Business.Abstractions;

public interface ISickLeaveService
{
    public Task<List<SickLeaveWithRelationsBusinessResponse>> GetSickLeavesAsync(DateTime date, Guid? userId, bool searchByYear = false);

    public Task<List<SickLeaveWithRelationsBusinessResponse>> GetUsersSickLeavesForMonthAsync(List<Guid> userIds, DateTime monthDate);

    public Task CreateSickLeaveAsync(SickLeaveBusinessRequest sickLeaveBusinessRequest);

    public Task UpdateSickLeaveAsync(Guid id, SickLeaveBusinessRequest sickLeaveBusinessRequest);

    public Task DeleteSickLeaveAsync(Guid id);
}