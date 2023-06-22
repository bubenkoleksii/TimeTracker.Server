using GraphQL.Types;
using TimeTracker.Server.GraphQl.User;

namespace TimeTracker.Server.GraphQl;

public sealed class RootQuery : ObjectGraphType
{
    public RootQuery()
    {
        Field<UserQuery>("user").Resolve(_ => new { });
    }
}