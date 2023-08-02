﻿using GraphQL.Types;
using TimeTracker.Server.Models.WorkSession;

namespace TimeTracker.Server.GraphQl.WorkSession.Types;

public class WorkSessionInputType : InputObjectGraphType<WorkSessionRequest>
{
    public WorkSessionInputType()
    {
        Field(ws => ws.UserId, type: typeof(IdGraphType), nullable: false)
            .Description("User id field for work session object");
        Field(ws => ws.Start, type: typeof(DateTimeGraphType), nullable: false)
            .Description("Start of work session field in work session object");
        Field(ws => ws.End, type: typeof(DateTimeGraphType), nullable: true)
            .Description("End of work session field in work session object");
        Field(ws => ws.Type, type: typeof(StringGraphType), nullable: false)
            .Description("Type field in work session object");
        Field(ws => ws.Title, type: typeof(StringGraphType), nullable: true)
            .Description("Title field in work session object");
        Field(ws => ws.Description, type: typeof(StringGraphType), nullable: true)
            .Description("Description field in work session object");
    }
}