using GraphQL.MicrosoftDI;
using GraphQL;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using AutoMapper;
using TimeTracker.Server.GraphQl.Holiday.Types;
using TimeTracker.Server.Models.Holiday;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.GraphQl.Holiday;

public class HolidayQuery : ObjectGraphType
{
    public HolidayQuery(IMapper mapper)
    {
        Field<HolidayType>("getHolidayById")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Resolve()
                .WithScope()
                .WithService<IHolidayService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");

                    var holidayBusinessResponse = await service.GetHolidayByIdAsync(id);
                    var holidayResponse = mapper.Map<HolidayResponse>(holidayBusinessResponse);

                    return holidayResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());

        Field<ListGraphType<HolidayType>>("getHolidays")
                .Resolve()
                .WithScope()
                .WithService<IHolidayService>()
                .ResolveAsync(async (context, service) =>
                {
                    var holidaysBusinessResponse = await service.GetHolidaysAsync();
                    var holidaysResponse = mapper.Map<IEnumerable<HolidayResponse>>(holidaysBusinessResponse);

                    return holidaysResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());

        Field<ListGraphType<HolidayType>>("getHolidaysForMonth")
                .Argument<NonNullGraphType<DateGraphType>>("monthDate")
                .Resolve()
                .WithScope()
                .WithService<IHolidayService>()
                .ResolveAsync(async (context, service) =>
                {
                    var monthDate = context.GetArgument<DateTime>("monthDate");

                    var holidaysBusinessResponse = await service.GetHolidaysForMonthAsync(monthDate);
                    var holidaysResponse = mapper.Map<List<HolidayResponse>>(holidaysBusinessResponse);

                    return holidaysResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());
    }
}