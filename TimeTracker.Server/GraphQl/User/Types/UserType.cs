using GraphQL.Types;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User.Types;

public sealed class UserType : ObjectGraphType<UserResponse>
{
    public UserType()
    {
        Name = "User";

        Field(i => i.Id, type: typeof(IdGraphType), nullable: false)
            .Description("Id field for user object");
        Field(i => i.Email, type: typeof(StringGraphType), nullable: false)
            .Description("Email field for user object");
        Field(i => i.HashPassword, type: typeof(StringGraphType), nullable: false)
            .Description("Hash password field for user object");
        Field(i => i.RefreshToken, type: typeof(StringGraphType), nullable: true)
            .Description("Refresh token field for user object");
    }
}