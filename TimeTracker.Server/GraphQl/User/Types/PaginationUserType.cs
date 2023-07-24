using GraphQL.Types;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User.Types;

public class PaginationUserType : ObjectGraphType<PaginationResponse<UserResponse>>
{
    public PaginationUserType()
    {
        Name = "PaginationUser";

        Field(x => x.Count, nullable: false).Description("Total count of items");
        Field<ListGraphType<UserType>>(
            name: "items",
            resolve: context => context.Source.Items,
            description: "List of items"
        );
    }
}
