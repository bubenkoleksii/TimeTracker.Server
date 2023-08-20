using GraphQL.Types;
using TimeTracker.Server.GraphQl.Holiday;
using TimeTracker.Server.GraphQl.SickLeave;
using TimeTracker.Server.GraphQl.User;
using TimeTracker.Server.GraphQl.Vacation;
using TimeTracker.Server.GraphQl.WorkSession;

namespace TimeTracker.Server.GraphQl;

public sealed class RootQuery : ObjectGraphType
{
    public RootQuery()
    {
        Field<UserQuery>("user").Resolve(_ => new { });

        Field<WorkSessionQuery>("workSession").Resolve(_ => new { });

        Field<HolidayQuery>("holiday").Resolve(_ => new { });

        Field<VacationQuery>("vacation").Resolve(_ => new { });

        Field<SickLeaveQuery>("sickLeave").Resolve(_ => new { });
    }
}