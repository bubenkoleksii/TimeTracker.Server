using GraphQL.Types;
using TimeTracker.Server.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation.Types;

public class VacationInputType : InputObjectGraphType<VacationRequest>
{
	public VacationInputType()
	{
        Field(v => v.UserId, type: typeof(IdGraphType), nullable: false)
                .Description("User id field for vacation object");
        Field(v => v.Start, type: typeof(DateGraphType), nullable: false)
                .Description("Start field for vacation object");
        Field(v => v.End, type: typeof(DateGraphType), nullable: false)
                .Description("End field for vacation object");
        Field(v => v.Comment, type: typeof(StringGraphType), nullable: true)
                .Description("Comment field for vacation object");
    }
}