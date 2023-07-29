using AutoMapper;
using GraphQL.Types;
using GraphQL.MicrosoftDI;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.WorkSession.Types;
using GraphQL;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Models.WorkSession;

namespace TimeTracker.Server.GraphQl.WorkSession
{
    public class WorkSessionMutation : ObjectGraphType
    {
        public WorkSessionMutation(IMapper mapper)
        {
            Field<WorkSessionType>("create")
                .Argument<NonNullGraphType<WorkSessionInputType>>("workSession")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var workSession = context.GetArgument<WorkSessionRequest>("workSession");
                    var workSessionBusinessRequest = mapper.Map<WorkSessionBusinessRequest>(workSession);
                    var workSessionBusinessResponse = await service.CreateWorkSessionAsync(workSessionBusinessRequest);
                    var workSessionResponse = mapper.Map<WorkSessionResponse>(workSessionBusinessResponse);
                    return workSessionResponse;
                }).AuthorizeWithPolicy("LoggedIn");

            Field<BooleanGraphType>("setEnd")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Argument<NonNullGraphType<DateTimeGraphType>>("endDateTime")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");
                    var endDateTime = context.GetArgument<DateTime>("endDateTime");
                    await service.SetWorkSessionEndAsync(id, endDateTime);
                    return true;
                }).AuthorizeWithPolicy("LoggedIn");

            Field<BooleanGraphType>("update")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Argument<NonNullGraphType<WorkSessionInputUpdateType>>("workSession")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");
                    var workSession = context.GetArgument<WorkSessionUpdateRequest>("workSession");
                    var workSessionBusinessRequest = mapper.Map<WorkSessionBusinessUpdateRequest>(workSession);
                    await service.UpdateWorkSessionAsync(id, workSessionBusinessRequest);
                    return true;
                }).AuthorizeWithPolicy("LoggedIn");

            Field<BooleanGraphType>("delete")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");
                    await service.DeleteWorkSessionAsync(id);
                    return true;
                }).AuthorizeWithPolicy("LoggedIn");
        }
    }
}