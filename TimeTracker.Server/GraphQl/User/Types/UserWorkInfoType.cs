using GraphQL.Types;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User.Types;

public class UserWorkInfoType : ObjectGraphType<UserWorkInfoResponse>
{
    public UserWorkInfoType()
    {
        Name = "UserWorkInfo";

        Field(i => i.UserId, type: typeof(IdGraphType), nullable: false)
            .Description("User id field for user work info object");
        Field(i => i.Email, type: typeof(StringGraphType), nullable: false)
            .Description("Email field for user work info object");
        Field(i => i.FullName, type: typeof(StringGraphType), nullable: false)
            .Description("Full name field for user work info object");
        Field(i => i.EmploymentRate, type: typeof(IntGraphType), nullable: false)
            .Description("Employment rate field for user work info object");
        Field(i => i.WorkedHours, type: typeof(FloatGraphType), nullable: false)
            .Description("Worked hours field for user work info object");
        Field(i => i.PlannedWorkingHours, type: typeof(FloatGraphType), nullable: false)
            .Description("Planned working hours field for user work info object");
        Field(i => i.SickLeaveHours, type: typeof(FloatGraphType), nullable: false)
            .Description("Sick leave hours field for user work info object");
        Field(i => i.VacationHours, type: typeof(FloatGraphType), nullable: false)
            .Description("Vacation hours field for user work info object");
    }
}