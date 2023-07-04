using GraphQL;
using GraphQL.Types;
using GraphQL.MicrosoftDI;
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
                var jwt = service.GetAccessToken();
                var claims = service.GetUserClaims(jwt);
                await service.RequireUserAuthorizationAsync(claims);
                return service.GetClaimValue(claims, "exp");
            });
    }
}