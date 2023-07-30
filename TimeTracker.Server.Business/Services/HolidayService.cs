using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Holiday;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Holidays;
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

    public async Task<HolidayBusinessResponse> CreateHolidayAsync(HolidayBusinessRequest holidayBusinessRequest)
    {
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
}