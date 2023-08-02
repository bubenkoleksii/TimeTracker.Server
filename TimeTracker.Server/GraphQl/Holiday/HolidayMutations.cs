using GraphQL.MicrosoftDI;
using GraphQL.Types;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.Holiday.Types;
using TimeTracker.Server.Models.Holiday;
using AutoMapper;
using TimeTracker.Server.Business.Models.Holiday;

namespace TimeTracker.Server.GraphQl.Holiday;

public class HolidayMutations : ObjectGraphType
{
    public HolidayMutations(IMapper mapper)
    {
        Field<HolidayType>("create")
                .Argument<NonNullGraphType<HolidayInputType>>("holiday")
                .Resolve()
                .WithScope()
                .WithService<IHolidayService>()
                .ResolveAsync(async (context, service) =>
                {
                    var holiday = context.GetArgument<HolidayRequest>("holiday");
                    var holidayBusinessRequest = mapper.Map<HolidayBusinessRequest>(holiday);

                    var holidayBusinessResponse = await service.CreateHolidayAsync(holidayBusinessRequest);
                    var holidayResponse = mapper.Map<HolidayResponse>(holidayBusinessResponse);

                    return holidayResponse;
                }).AuthorizeWithPolicy("LoggedIn");

        Field<BooleanGraphType>("update")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Argument<NonNullGraphType<HolidayInputType>>("holiday")
                .Resolve()
                .WithScope()
                .WithService<IHolidayService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");
                    var holiday = context.GetArgument<HolidayRequest>("holiday");
                    var holidayBusinessRequest = mapper.Map<HolidayBusinessRequest>(holiday);

                    await service.UpdateHolidayAsync(id, holidayBusinessRequest);

                    return true;
                }).AuthorizeWithPolicy("LoggedIn");

        Field<BooleanGraphType>("delete")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Resolve()
                .WithScope()
                .WithService<IHolidayService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");
                    await service.DeleteHolidayAsync(id);
                    return true;
                }).AuthorizeWithPolicy("LoggedIn");
    }
}