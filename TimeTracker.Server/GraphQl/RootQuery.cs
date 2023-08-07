using GraphQL.Types;
using TimeTracker.Server.GraphQl.Holiday;
using TimeTracker.Server.GraphQl.User;
using TimeTracker.Server.GraphQl.WorkSession;

namespace TimeTracker.Server.GraphQl;

public sealed class RootQuery : ObjectGraphType
{
    public RootQuery()
    {
        Field<UserQuery>("user").Resolve(_ => new { });

        Field<WorkSessionQuery>("workSession").Resolve(_ => new { });

        Field<HolidayQuery>("holiday").Resolve(_ => new { });
    }
}