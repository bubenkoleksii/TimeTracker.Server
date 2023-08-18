using GraphQL.Types;
using TimeTracker.Server.Models.SickLeave;

namespace TimeTracker.Server.GraphQl.SickLeave.Types;

public class SickLeaveInputType : InputObjectGraphType<SickLeaveRequest>
{
    public SickLeaveInputType()
    {
        Field(sl => sl.UserId, type: typeof(IdGraphType), nullable: false)
                .Description("User id field for sick leave object");
        Field(sl => sl.LastModifierId, type: typeof(IdGraphType), nullable: false)
                .Description("Last modifier id field for sick leave object");
        Field(sl => sl.Start, type: typeof(DateGraphType), nullable: false)
                .Description("Start date field for sick leave object");
        Field(sl => sl.End, type: typeof(DateGraphType), nullable: false)
                .Description("End date field for sick leave object");
    }
}