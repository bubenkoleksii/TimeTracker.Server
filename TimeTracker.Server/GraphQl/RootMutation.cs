using GraphQL.Types;
using TimeTracker.Server.GraphQl.User;

namespace TimeTracker.Server.GraphQl;

public sealed class RootMutation : ObjectGraphType
{
    public RootMutation()
    {
        Field<UserMutation>("user").Resolve(_ => new { });
    }
}