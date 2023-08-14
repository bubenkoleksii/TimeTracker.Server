using GraphQL.Types;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User.Types;

public class PaginationProfileType : ObjectGraphType<PaginationResponse<ProfileResponse>>
{
    public PaginationProfileType()
    {
        Name = "PaginationProfile";

        Field(x => x.Count, nullable: false).Description("Total count of items");
        Field<ListGraphType<ProfileType>>(
            name: "items",
            resolve: context => context.Source.Items,
            description: "List of items"
        );
    }
}