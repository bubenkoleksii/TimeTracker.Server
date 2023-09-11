using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.SickLeave.Types;
using TimeTracker.Server.Models.SickLeave;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.GraphQl.SickLeave;

public class SickLeaveQuery : ObjectGraphType
{
    public SickLeaveQuery(IMapper mapper)
    {
        Field<ListGraphType<SickLeaveType>>("getSickLeaves")
                .Argument<NonNullGraphType<DateGraphType>>("date")
                .Argument<IdGraphType>("userId")
                .Argument<BooleanGraphType>("searchByYear")
                .Resolve()
                .WithScope()
                .WithService<ISickLeaveService>()
                .ResolveAsync(async (context, service) =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var userId = context.GetArgument<Guid?>("userId");
                    var searchByYear = context.GetArgument<bool?>("searchByYear");

                    var sickLeaveWithRelationsBusinessResponses = await service.GetSickLeavesAsync(date, userId, searchByYear is not null && (bool)searchByYear);
                    var sickLeaveWithRelationsResponses = mapper.Map<List<SickLeaveResponse>>(sickLeaveWithRelationsBusinessResponses);

                    return sickLeaveWithRelationsResponses;
                }).AuthorizeWithPolicy(nameof(PermissionsEnum.LoggedIn));

        Field<ListGraphType<SickLeaveType>>("getUsersSickLeavesForMonth")
                .Argument<NonNullGraphType<ListGraphType<IdGraphType>>>("userIds")
                .Argument<NonNullGraphType<DateGraphType>>("monthDate")
                .Resolve()
                .WithScope()
                .WithService<ISickLeaveService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userIds = context.GetArgument<List<Guid>>("userIds");
                    var monthDate = context.GetArgument<DateTime>("monthDate");

                    var sickLeaveWithRelationsBusinessResponses = await service.GetUsersSickLeavesForMonthAsync(userIds, monthDate);
                    var sickLeaveWithRelationsResponses = mapper.Map<List<SickLeaveResponse>>(sickLeaveWithRelationsBusinessResponses);

                    return sickLeaveWithRelationsResponses;
                }).AuthorizeWithPolicy(nameof(PermissionsEnum.LoggedIn));
    }
}