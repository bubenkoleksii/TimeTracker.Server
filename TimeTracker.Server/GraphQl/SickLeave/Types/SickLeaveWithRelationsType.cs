using GraphQL.Types;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.SickLeave;

namespace TimeTracker.Server.GraphQl.SickLeave.Types;

public class SickLeaveWithRelationsType : ObjectGraphType<SickLeaveWithRelationsResponse>
{
    public SickLeaveWithRelationsType()
    {
        Field(sl => sl.SickLeave, type: typeof(SickLeaveType), nullable: false)
                .Description("Sick leave data field for sick leave object");
        Field(sl => sl.User, type: typeof(UserType), nullable: false)
                .Description("User data field for sick leave object");
        Field(sl => sl.LastModifier, type: typeof(UserType), nullable: false)
                .Description("Last modifier data field for sick leave object");
    }
}