using GraphQL.Types;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation.Types;

public class VacationWithUserType : ObjectGraphType<VacationWithUserResponse>
{
    public VacationWithUserType()
    {
        Field(v => v.Vacation, type: typeof(VacationType), nullable: false)
                .Description("Vacation field for vacation object");
        Field(v => v.User, type: typeof(UserType), nullable: false)
                .Description("Vacation user field for vacation object");
        Field(v => v.Approver, type: typeof(UserType), nullable: true)
                .Description("Approver user field for vacation object");
    }
}