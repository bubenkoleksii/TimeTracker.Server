using AutoMapper;
using GraphQL.Types;
using GraphQL.MicrosoftDI;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.WorkSession.Types;
using GraphQL;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.WorkSession;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.GraphQl.WorkSession
{
    public class WorkSessionQuery : ObjectGraphType
    {
        public WorkSessionQuery(IMapper mapper)
        {
            Field<WorkSessionPaginationResponseType>("getWorkSessionsByUserId")
                .Argument<NonNullGraphType<IdGraphType>>("userId")
                .Argument<BooleanGraphType>("orderByDesc")
                .Argument<IntGraphType>("offset")
                .Argument<IntGraphType>("limit")
                .Argument<DateTimeGraphType>("startDate")
                .Argument<DateTimeGraphType>("endDate")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userId = context.GetArgument<Guid>("userId");
                    var orderByDesc = context.GetArgument<bool?>("orderByDesc");
                    var offset = context.GetArgument<int?>("offset");
                    var limit = context.GetArgument<int?>("limit");
                    var startDate = context.GetArgument<DateTime?>("startDate");
                    var endDate = context.GetArgument<DateTime?>("endDate");

                    var workSessionPaginationBusinessResponse = await service.GetWorkSessionsByUserIdAsync(userId, orderByDesc, offset, limit, startDate, endDate);
                    var workSessionPaginationResponse = mapper.Map<PaginationResponse<WorkSessionWithRelationsResponse>>(workSessionPaginationBusinessResponse);

                    return workSessionPaginationResponse;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());

            Field<WorkSessionType>("getActiveWorkSessionByUserId")
                .Argument<NonNullGraphType<IdGraphType>>("userId")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userId = context.GetArgument<Guid>("userId");
                    var workSessionBusinessResponse = await service.GetActiveWorkSessionByUserIdAsync(userId);
                    var workSession = mapper.Map<WorkSessionResponse>(workSessionBusinessResponse);
                    return workSession;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());
        }
    }
}