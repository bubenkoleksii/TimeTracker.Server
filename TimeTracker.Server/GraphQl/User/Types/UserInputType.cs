using GraphQL.Types;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User.Types;

public sealed class UserInputType : InputObjectGraphType<UserRequest>
{
    public UserInputType()
    {
        Name = "UserInput";

        Field(i => i.Email, type: typeof(StringGraphType), nullable: false)
            .Description("Email field for user object");
        Field(i => i.Password, type: typeof(StringGraphType), nullable: false)
            .Description("Password field for user object");
    }
}