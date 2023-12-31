﻿using TimeTracker.Server.Data.Models.Holidays;

namespace TimeTracker.Server.Data.Abstractions;

public interface IHolidayRepository
{
    public Task<HolidayDataResponse> GetHolidayByIdAsync(Guid id);

    public Task<IEnumerable<HolidayDataResponse>> GetHolidaysAsync();

    public Task<List<HolidayDataResponse>> GetHolidaysByDateAsync(DateOnly date, HolidayTypesEnum? holidayType = null);

    public Task<List<HolidayDataResponse>> GetHolidaysByDateRangeAsync(DateOnly start, DateOnly end);

    public Task<HolidayDataResponse> CreateHolidayAsync(HolidayDataRequest holidayDataRequest);

    public Task UpdateHolidayAsync(Guid id, HolidayDataRequest holidayDataRequest);

    public Task DeleteHolidayAsync(Guid id);

}