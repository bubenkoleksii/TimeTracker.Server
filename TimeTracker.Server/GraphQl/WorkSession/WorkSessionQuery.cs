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
            Field<ListGraphType<WorkSessionType>>("getWorkSessionsByUserId")
                .Argument<NonNullGraphType<IdGraphType>>("userId")
                .Resolve()
                .WithScope()
                .WithService<IWorkSessionService>()
                .ResolveAsync(async (context, service) =>
                {
                    var userId = context.GetArgument<Guid>("userId");
                    var workSessionsBusinessResponse = await service.GetWorkSessionsByUserId(userId);
                    var workSessions = mapper.Map<IEnumerable<WorkSessionResponse>>(workSessionsBusinessResponse);
                    return workSessions;
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