using GraphQL.Types;
using TimeTracker.Server.Models.Auth;

namespace TimeTracker.Server.GraphQl.Auth.Types;

public sealed class AuthType : ObjectGraphType<AuthResponse>
{
    public AuthType()
    {
        Name = "Auth";

        Field(i => i.AccessToken, type: typeof(StringGraphType), nullable: false)
            .Description("Access token field for auth object");
        Field(i => i.RefreshToken, type: typeof(StringGraphType), nullable: false)
            .Description("Refresh token field for auth object");
    }
}