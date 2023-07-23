using AutoMapper;
using GraphQL.Types;
using GraphQL.MicrosoftDI;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.WorkSession.Types;
using GraphQL;
using TimeTracker.Server.Models.WorkSession;

namespace TimeTracker.Server.GraphQl.WorkSession
{
    public class WorkSessionQuery : ObjectGraphType
    {
        public WorkSessionQuery(IMapper mapper)
        {
            Field<WorkSessionPaginationResponseType>("getWorkSessionsByUserId")
                .Argument<NonNullGraphType<IdGraphType>>("userId")
                .Argument<NonNullGraphType<BooleanGraphType>>("orderByDesc")
                .Argument<NonNullGraphType<IntGraphType>>("offset")
                .Argument<NonNullGraphType<IntGraphType>>("limit")
                .Argument<DateTimeGraphType>("filterDate")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userId = context.GetArgument<Guid>("userId");
                    var orderByDesc = context.GetArgument<bool>("orderByDesc");
                    var offset = context.GetArgument<int>("offset");
                    var limit = context.GetArgument<int>("limit");
                    var filterDate = context.GetArgument<DateTime?>("filterDate");
                    var workSessionPaginationBusinessResponse = await service.GetWorkSessionsByUserId(userId, orderByDesc, offset, limit, filterDate);
                    var workSessionPaginationResponse = mapper.Map<WorkSessionPaginationResponse<WorkSessionResponse>>(workSessionPaginationBusinessResponse);
                    return workSessionPaginationResponse;
                }).AuthorizeWithPolicy("LoggedIn"); ;

            Field<WorkSessionType>("getWorkSessionById")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");
                    var workSessionBusinessResponse = await service.GetWorkSessionById(id);
                    var workSession = mapper.Map<WorkSessionResponse>(workSessionBusinessResponse);
                    return workSession;
                }).AuthorizeWithPolicy("LoggedIn"); ;

            Field<WorkSessionType>("getActiveWorkSessionByUserId")
                .Argument<NonNullGraphType<IdGraphType>>("userId")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userId = context.GetArgument<Guid>("userId");
                    var workSessionBusinessResponse = await service.GetActiveWorkSessionByUserId(userId);
                    var workSession = mapper.Map<WorkSessionResponse>(workSessionBusinessResponse);
                    return workSession;
                }).AuthorizeWithPolicy("LoggedIn"); ;
        }
    }
}