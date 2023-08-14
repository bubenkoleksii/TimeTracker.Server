using GraphQL.Types;
using TimeTracker.Server.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation.Types;

public class VacationType : ObjectGraphType<VacationResponse>
{
    public VacationType()
    {
        Field(v => v.Id, type: typeof(IdGraphType), nullable: false)
                .Description("Id field for vacation object");
        Field(v => v.UserId, type: typeof(IdGraphType), nullable: false)
                .Description("User id field for vacation object");
        Field(v => v.Start, type: typeof(DateGraphType), nullable: false)
                .Description("Start field for vacation object");
        Field(v => v.End, type: typeof(DateGraphType), nullable: false)
                .Description("End field for vacation object");
        Field(v => v.Comment, type: typeof(StringGraphType), nullable: true)
                .Description("Comment field for vacation object");
        Field(v => v.IsApproved, type: typeof(BooleanGraphType), nullable: false)
                .Description("Is approved field for vacation object");
        Field(v => v.ApproverId, type: typeof(IdGraphType), nullable: false)
                .Description("Approver id field for vacation object");
        Field(v => v.ApproverComment, type: typeof(StringGraphType), nullable: true)
                .Description("Approver comment field for vacation object");
    }
}