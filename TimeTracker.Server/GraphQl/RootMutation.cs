using GraphQL.Types;
using TimeTracker.Server.GraphQl.Auth;
using TimeTracker.Server.GraphQl.Holiday;
using TimeTracker.Server.GraphQl.SickLeave;
using TimeTracker.Server.GraphQl.User;
using TimeTracker.Server.GraphQl.Vacation;
using TimeTracker.Server.GraphQl.WorkSession;

namespace TimeTracker.Server.GraphQl;

public sealed class RootMutation : ObjectGraphType
{
    public RootMutation()
    {
        Field<UserMutation>("user").Resolve(_ => new { });

        Field<AuthMutation>("auth").Resolve(_ => new { });

        Field<WorkSessionMutation>("workSession").Resolve(_ => new { });

        Field<HolidayMutations>("holiday").Resolve(_ => new { });

        Field<VacationMutation>("vacation").Resolve(_ => new { });

        Field<SickLeaveMutation>("sickLeave").Resolve(_ => new { });
    }
}