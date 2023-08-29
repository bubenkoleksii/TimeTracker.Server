using AutoMapper;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Shared;
using TimeTracker.Server.GraphQl.Vacation.Types;
using TimeTracker.Server.Models.Vacation;
using TimeTracker.Server.Business.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation;

public class VacationQuery : ObjectGraphType
{
    public VacationQuery(IMapper mapper)
    {
        Field<ListGraphType<VacationWithUserType>>("getVacationsByUserId")
                .Argument<NonNullGraphType<IdGraphType>>("userId")
                .Argument<BooleanGraphType>("onlyApproved")
                .Argument<NonNullGraphType<BooleanGraphType>>("orderByDesc")
                .Resolve()
                .WithScope()
                .WithService<IVacationService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userId = context.GetArgument<Guid>("userId");
                    var onlyApproved = context.GetArgument<bool?>("onlyApproved");
                    var orderByDesc = context.GetArgument<bool>("orderByDesc");

                    var vacationWithUserBusinessResponses = await service.GetVacationsByUserIdAsync(userId, onlyApproved, orderByDesc);
                    var vacationWithUserResponse = mapper.Map<IEnumerable<VacationWithUserResponse>>(vacationWithUserBusinessResponses);

                    return vacationWithUserResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());

        Field<ListGraphType<VacationWithUserType>>("getUsersVacationsForMonth")
                .Argument<NonNullGraphType<ListGraphType<IdGraphType>>>("userIds")
                .Argument<NonNullGraphType<DateGraphType>>("monthDate")
                .Resolve()
                .WithScope()
                .WithService<IVacationService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userIds = context.GetArgument<List<Guid>>("userIds");
                    var monthDate = context.GetArgument<DateTime>("monthDate");

                    var vacationWithUserBusinessResponses = await service.GetUsersVacationsForMonth(userIds, monthDate);
                    var vacationWithUserResponse = mapper.Map<List<VacationWithUserResponse>>(vacationWithUserBusinessResponses);

                    return vacationWithUserResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());

        Field<ListGraphType<VacationWithUserType>>("getVacationsRequests")
                .Argument<NonNullGraphType<BooleanGraphType>>("getNotStarted")
                .Resolve()
                .WithScope()
                .WithService<IVacationService>()
                .ResolveAsync(async (context, service) =>
                {
                    var getNotStarted = context.GetArgument<bool>("getNotStarted");

                    var vacationWithUserBusinessResponses = await service.GetVacationRequestsAsync(getNotStarted);
                    var vacationWithUserResponse = mapper.Map<IEnumerable<VacationWithUserResponse>>(vacationWithUserBusinessResponses);

                    return vacationWithUserResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString())
                  .AuthorizeWithPolicy(PermissionsEnum.GetVacations.ToString());

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