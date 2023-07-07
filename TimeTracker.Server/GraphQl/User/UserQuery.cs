using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;

namespace TimeTracker.Server.GraphQl.User;

public class UserQuery : ObjectGraphType
{
    public UserQuery()
    {
        Field<StringGraphType>("test")
            .Resolve()
            .WithScope()
            .WithService<IJwtService>()
            .ResolveAsync(async (context, service) =>
            {
                return "aoaoao";
            }).AuthorizeWithPolicy("LoggedIn");
    }
}