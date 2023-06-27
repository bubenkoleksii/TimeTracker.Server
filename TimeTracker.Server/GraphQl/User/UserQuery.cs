using GraphQL.Types;

namespace TimeTracker.Server.GraphQl.User;

public class UserQuery : ObjectGraphType
{
    public UserQuery()
    {
        Field<StringGraphType>("test")
            .Resolve(context =>
            {
                return "some";
            });
    }
}