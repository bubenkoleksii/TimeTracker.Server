using GraphQL.Types;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.WorkSession;

namespace TimeTracker.Server.GraphQl.WorkSession.Types
{
    public class WorkSessionPaginationResponseType : ObjectGraphType<PaginationResponse<WorkSessionWithRelationsResponse>>
    {
        public WorkSessionPaginationResponseType()
        {
            Field(x => x.Count, nullable: false).Description("Total count of items");
            Field<ListGraphType<WorkSessionWithRelationsType>>(
                name: "items",
                resolve: context => context.Source.Items,
                description: "List of items"
            );
        }
    }
}