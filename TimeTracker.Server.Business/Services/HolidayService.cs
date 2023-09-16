using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Holiday;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Holidays;
using TimeTracker.Server.Data.Repositories;
using TimeTracker.Server.Shared.Exceptions;

namespace TimeTracker.Server.Business.Services;

public class HolidayService : IHolidayService
{
    private readonly IMapper _mapper;
    private readonly IHolidayRepository _holidayRepository;

    public HolidayService(IMapper mapper, IHolidayRepository holidayRepository, IUserRepository userRepository)
    {
        _mapper = mapper;
        _holidayRepository = holidayRepository;
    }

    public async Task<HolidayBusinessResponse> GetHolidayByIdAsync(Guid id)
    {
        var holidayDataResponse = await _holidayRepository.GetHolidayByIdAsync(id);
        var holidayBusinessResponse = _mapper.Map<HolidayBusinessResponse>(holidayDataResponse);
        return holidayBusinessResponse;
    }

    public async Task<IEnumerable<HolidayBusinessResponse>> GetHolidaysAsync()
    {
        var holidaysDataResponse = await _holidayRepository.GetHolidaysAsync();
        var holidaysBusinessResponse = _mapper.Map<IEnumerable<HolidayBusinessResponse>>(holidaysDataResponse);
        return holidaysBusinessResponse;
    }

    public async Task<List<HolidayBusinessResponse>> GetHolidaysForMonthAsync(DateTime monthDate)
    {
        var startDate = new DateOnly(monthDate.Year, monthDate.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(7);
        startDate = startDate.AddDays(-7);

        var holidayDataResponseList = await _holidayRepository.GetHolidaysByDateRangeAsync(startDate, endDate);
        var holidayBusinessResponseList = _mapper.Map<List<HolidayBusinessResponse>>(holidayDataResponseList);

        return holidayBusinessResponseList;
    }

    public async Task<HolidayBusinessResponse> CreateHolidayAsync(HolidayBusinessRequest holidayBusinessRequest)
    {
        ValidateHolidayRequestData(holidayBusinessRequest);
        var holidayDataRequest = _mapper.Map<HolidayDataRequest>(holidayBusinessRequest);
        var holidayDataResponse = await _holidayRepository.CreateHolidayAsync(holidayDataRequest);
        var holidayBusinessResponse = _mapper.Map<HolidayBusinessResponse>(holidayDataResponse);
        return holidayBusinessResponse;
    }

    public async Task UpdateHolidayAsync(Guid id, HolidayBusinessRequest holidayBusinessRequest)
    {
        var holiday = await _holidayRepository.GetHolidayByIdAsync(id);
        if (holiday is null)
        {
            throw new ExecutionError("Holiday not found")
            {
                Code = GraphQLCustomErrorCodesEnum.HOLIDAY_NOT_FOUND.ToString()
            };
        }
        ValidateHolidayRequestData(holidayBusinessRequest);

        var holidayDataRequest = _mapper.Map<HolidayDataRequest>(holidayBusinessRequest);
        await _holidayRepository.UpdateHolidayAsync(id, holidayDataRequest);
    }

    public async Task DeleteHolidayAsync(Guid id)
    {
        var holiday = await _holidayRepository.GetHolidayByIdAsync(id);
        if (holiday is null)
        {
            throw new ExecutionError("Holiday not found")
            {
                Code = GraphQLCustomErrorCodesEnum.HOLIDAY_NOT_FOUND.ToString()
            };
        }

        await _holidayRepository.DeleteHolidayAsync(id);
    }

    public async Task<CountOfWorkingDaysBusinessResponse> GetCountOfWorkingDays(DateOnly start, DateOnly end)
    {
        var holidaysDataResponse = await _holidayRepository.GetHolidaysByDateRangeAsync(start, end);

        var countOfHolidayDays = 0;
        var counterOfShortDays = 0;

        var holidaysDataResponseArray = holidaysDataResponse.ToArray();
        for (var i = 0; i < holidaysDataResponseArray.Length; i++)
        {
            var holiday = holidaysDataResponseArray[i];
            var previousHoliday = i != 0 ? holidaysDataResponseArray[i - 1] : null;

            if (holiday.Date.DayOfWeek != DayOfWeek.Sunday && holiday.Date.DayOfWeek != DayOfWeek.Monday)
            {
                if (previousHoliday == null)
                {
                    counterOfShortDays++;
                }
                else
                {
                    if ((holiday.Date - previousHoliday.Date).Days > 1)
                    {
                        counterOfShortDays++;
                    }
                }
            }

            if (holiday.EndDate != null)
            {
                for (var date = holiday.Date; date <= holiday.EndDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                    {
                        countOfHolidayDays++;
                    }
                }
            }
            else
            {
                if (holiday.Date.DayOfWeek != DayOfWeek.Sunday && holiday.Date.DayOfWeek != DayOfWeek.Saturday)
                {
                    countOfHolidayDays++;
                }
            }
        }

        var countOfFullDays = end.DayNumber - start.DayNumber + 1;
        countOfFullDays -= countOfHolidayDays;

        var countOfWeekendDays = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
            {
                countOfWeekendDays++;
            }
        }

        countOfFullDays -= countOfWeekendDays;

        return new CountOfWorkingDaysBusinessResponse
        {
            FullDays = countOfFullDays,
            ShortDays = counterOfShortDays
        };
    }

    protected void ValidateHolidayRequestData(HolidayBusinessRequest holidayBusinessRequest)
    {
        if (!Enum.TryParse<HolidayTypesEnum>(holidayBusinessRequest.Type, false, out var _))
        {
            throw new ExecutionError("Invalid holiday type")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_INPUT_DATA.ToString()
            };
        }

        if (holidayBusinessRequest.EndDate is not null && DateTime.Compare(holidayBusinessRequest.Date, (DateTime)holidayBusinessRequest.EndDate) > 0)
        {
            throw new ExecutionError("Invalid dates input")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_INPUT_DATA.ToString()
            };
        }
    }
}