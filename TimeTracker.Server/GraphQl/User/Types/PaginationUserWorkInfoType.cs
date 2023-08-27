using GraphQL.Types;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User.Types;

public class PaginationUserWorkInfoType : ObjectGraphType<PaginationResponse<UserWorkInfoResponse>>
{
    public PaginationUserWorkInfoType()
    {
        Name = "PaginationUsersWorkInfo";

        Field(x => x.Count, nullable: false).Description("Total count of items");
        Field<ListGraphType<UserWorkInfoType>>(
            name: "items",
            resolve: context => context.Source.Items,
            description: "List of items"
        );
    }
}