using GraphQL.Types;
using TimeTracker.Server.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation.Types;

public class VacationApproveInputType : InputObjectGraphType<VacationApproveRequest>
{
    public VacationApproveInputType()
    {
        Field(v => v.Id, type: typeof(IdGraphType), nullable: false)
                .Description("Id field for vacation object");
        Field(v => v.IsApproved, type: typeof(BooleanGraphType), nullable: false)
                .Description("Is approved field for vacation object");
        Field(v => v.ApproverId, type: typeof(IdGraphType), nullable: false)
                .Description("Approver id field for vacation object");
        Field(v => v.ApproverComment, type: typeof(StringGraphType), nullable: true)
                .Description("Approver comment field for vacation object");
    }
}