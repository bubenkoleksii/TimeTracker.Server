using GraphQL.Types;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User.Types;

public class ProfileType : ObjectGraphType<ProfileResponse>
{
    public ProfileType()
    {
        Name = "Profile";

        Field(i => i.Id, type: typeof(IdGraphType), nullable: false)
            .Description("Id field for user object");
        Field(i => i.Email, type: typeof(StringGraphType), nullable: false)
            .Description("Email field for user object");
        Field(i => i.FullName, type: typeof(StringGraphType), nullable: false)
            .Description("Full name for user object");
        Field(i => i.Status, type: typeof(StringGraphType), nullable: false)
            .Description("Status for user object");
    }
}