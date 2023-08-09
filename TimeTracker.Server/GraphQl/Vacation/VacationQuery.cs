using AutoMapper;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Shared;
using TimeTracker.Server.GraphQl.Vacation.Types;
using TimeTracker.Server.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation;

public class VacationQuery : ObjectGraphType
{
    public VacationQuery(IMapper mapper)
    {
        Field<VacationInfoType>("getVacationInfoByUserId")
                .Argument<NonNullGraphType<IdGraphType>>("userId")
                .Resolve()
                .WithScope()
                .WithService<IVacationService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userId = context.GetArgument<Guid>("userId");

                    var vacationInfoBusinessResponse = await service.GetVacationInfoByUserIdAsync(userId);
                    var vacationInfoResponse = mapper.Map<VacationInfoResponse>(vacationInfoBusinessResponse);

                    return vacationInfoResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());
    }
}