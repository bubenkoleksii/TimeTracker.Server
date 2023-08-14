using GraphQL.Types;
using TimeTracker.Server.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation.Types;

public class VacationInfoType : ObjectGraphType<VacationInfoResponse>
{
    public VacationInfoType()
    {
        Field(vi => vi.UserId, type: typeof(IdGraphType), nullable: false)
                .Description("Id field for vacation info object");
        Field(vi => vi.EmploymentDate, type: typeof(DateGraphType), nullable: false)
                .Description("Employment Date field for vacation info object");
        Field(vi => vi.DaysSpent, type: typeof(IntGraphType), nullable: false)
                .Description("Days spent field for vacation info object");
    }
}