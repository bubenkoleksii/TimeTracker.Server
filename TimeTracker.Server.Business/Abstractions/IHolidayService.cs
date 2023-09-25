using TimeTracker.Server.Business.Models.Holiday;

namespace TimeTracker.Server.Business.Abstractions;

public interface IHolidayService
{
    public Task<HolidayBusinessResponse> GetHolidayByIdAsync(Guid id);

    public Task<IEnumerable<HolidayBusinessResponse>> GetHolidaysAsync();

    public Task<IEnumerable<DateTime>> GetLastDaysOfMonth(int limitYear = 2030, int limitMonth = 12);

    public Task<List<HolidayBusinessResponse>> GetHolidaysForMonthAsync(DateTime monthDate);

    public Task<HolidayBusinessResponse> CreateHolidayAsync(HolidayBusinessRequest holidayBusinessRequest);

    public Task UpdateHolidayAsync(Guid id, HolidayBusinessRequest holidayBusinessRequest);

    public Task DeleteHolidayAsync(Guid id);

    public Task<CountOfWorkingDaysBusinessResponse> GetCountOfWorkingDays(DateOnly start, DateOnly end);
}