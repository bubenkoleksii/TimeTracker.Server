using TimeTracker.Server.Business.Models.Holiday;

namespace TimeTracker.Server.Business.Abstractions;

public interface IHolidayService
{
    public Task<HolidayBusinessResponse> GetHolidayByIdAsync(Guid id);
    public Task<IEnumerable<HolidayBusinessResponse>> GetHolidaysAsync();
    public Task<HolidayBusinessResponse> CreateHolidayAsync(HolidayBusinessRequest holidayBusinessRequest);
    public Task UpdateHolidayAsync(Guid id, HolidayBusinessRequest holidayBusinessRequest);
    public Task DeleteHolidayAsync(Guid id);
}