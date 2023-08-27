using GraphQL.Types;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.WorkSession;

namespace TimeTracker.Server.GraphQl.WorkSession.Types;

public class WorkSessionWithRelationsType : ObjectGraphType<WorkSessionWithRelationsResponse>
{
    public WorkSessionWithRelationsType()
    {
        Field(ws => ws.WorkSession, type: typeof(WorkSessionType), nullable: false)
            .Description("Work session field in work session with relations object");
        Field(ws => ws.User, type: typeof(UserType), nullable: false)
            .Description("User field in work session with relations object");
        Field(ws => ws.LastModifier, type: typeof(UserType), nullable: false)
            .Description("Last modifier field in work session with relations object");
    }
}