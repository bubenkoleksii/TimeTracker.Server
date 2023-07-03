using GraphQL.Types;
using TimeTracker.Server.Models.Auth;

namespace TimeTracker.Server.GraphQl.Auth.Types;

public sealed class AuthInputType : InputObjectGraphType<AuthRequest>
{
    public AuthInputType()
    {
        Name = "AuthInput";

        Field(i => i.Email, type: typeof(StringGraphType), nullable: false)
            .Description("Email field for auth object");
        Field(i => i.Password, type: typeof(StringGraphType), nullable: false)
            .Description("Password field for auth object");
    }
}