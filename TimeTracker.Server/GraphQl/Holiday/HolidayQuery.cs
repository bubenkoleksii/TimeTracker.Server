using GraphQL.MicrosoftDI;
using GraphQL;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using AutoMapper;
using TimeTracker.Server.GraphQl.Holiday.Types;
using TimeTracker.Server.Models.Holiday;

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
                }).AuthorizeWithPolicy("LoggedIn");

        Field<ListGraphType<HolidayType>>("getHolidays")
                .Resolve()
                .WithScope()
                .WithService<IHolidayService>()
                .ResolveAsync(async (context, service) =>
                {
                    var holidaysBusinessResponse = await service.GetHolidaysAsync();
                    var holidaysResponse = mapper.Map<IEnumerable<HolidayResponse>>(holidaysBusinessResponse);

                    return holidaysResponse;
                }).AuthorizeWithPolicy("LoggedIn");
    }
}