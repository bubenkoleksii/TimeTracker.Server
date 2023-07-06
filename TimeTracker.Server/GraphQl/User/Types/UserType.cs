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
        Field(i => i.RefreshToken, type: typeof(StringGraphType), nullable: true)
            .Description("Refresh token field for user object");
        Field(i => i.FullName, type: typeof(StringGraphType), nullable: false)
            .Description("Full name for user object");
        Field(i => i.EmploymentRate, type: typeof(IntGraphType), nullable: false)
            .Description("Employment rate for user object");
        Field(i => i.Permissions, type: typeof(StringGraphType), nullable: true)
            .Description("Permissions for user object");
        Field(i => i.Status, type: typeof(StringGraphType), nullable: false)
            .Description("Status for user object");
    }
}