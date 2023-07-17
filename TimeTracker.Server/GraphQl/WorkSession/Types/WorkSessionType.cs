﻿using GraphQL.Types;
using TimeTracker.Server.Models.WorkSession;

namespace TimeTracker.Server.GraphQl.WorkSession.Types
{
    public class WorkSessionType : ObjectGraphType<WorkSessionResponse>
    {
        public WorkSessionType()
        {
            Field(ws => ws.Id, type: typeof(IdGraphType), nullable: false)
                .Description("Id field for work session object");
            Field(ws => ws.UserId, type: typeof(IdGraphType), nullable: false)
                .Description("User id field for work session object");
            Field(ws => ws.Start, type: typeof(DateTimeGraphType), nullable: false)
                .Description("Start of work session field in work session object");
            Field(ws => ws.End, type: typeof(DateTimeGraphType), nullable: true)
                .Description("End of work session field in work session object");
        }
    }
}