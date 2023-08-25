using GraphQL.Types;
using TimeTracker.Server.Models.WorkSession;

namespace TimeTracker.Server.GraphQl.WorkSession.Types;

public class WorkSessionInputUpdateType : InputObjectGraphType<WorkSessionUpdateRequest>
{
    public WorkSessionInputUpdateType()
    {
        Field(ws => ws.Start, type: typeof(DateTimeGraphType), nullable: false)
            .Description("Start of work session field in work session object");
        Field(ws => ws.End, type: typeof(DateTimeGraphType), nullable: false)
            .Description("End of work session field in work session object");
        Field(ws => ws.Title, type: typeof(StringGraphType), nullable: true)
            .Description("Title field in work session object");
        Field(ws => ws.Description, type: typeof(StringGraphType), nullable: true)
            .Description("Description field in work session object");
        Field(ws => ws.LastModifierId, type: typeof(IdGraphType), nullable: false)
            .Description("Last modifier id field in work session object");
    }
}