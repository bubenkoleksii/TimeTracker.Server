using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User;

public class UserQuery : ObjectGraphType
{
    public UserQuery(IMapper mapper)
    {
        Field<PaginationUserType>("getAll")
            .Argument<IntGraphType>("offset")
            .Argument<IntGraphType>("limit")
            .Argument<StringGraphType>("search")
            .Argument<StringGraphType>("sortingColumn")
            .Argument<IntGraphType>("filteringEmploymentRate")
            .Argument<StringGraphType>("filteringStatus")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var offset = context.GetArgument<int?>("offset");
                var limit = context.GetArgument<int?>("limit");
                var search = context.GetArgument<string>("search");
                var filteringEmploymentRate = context.GetArgument<int?>("filteringEmploymentRate");
                var filteringStatus = context.GetArgument<string?>("filteringStatus");
                var sortingColumn = context.GetArgument<string?>("sortingColumn");

                var usersBusinessResponse = await service.GetAllUsersAsync(offset, limit, search, filteringEmploymentRate, filteringStatus, sortingColumn);

                var usersResponse = mapper.Map<PaginationResponse<UserResponse>>(usersBusinessResponse);
                return usersResponse;
            }).AuthorizeWithPolicy("GetUsers");

        Field<PaginationProfileType>("getAllProfiles")
            .Argument<IntGraphType>("offset")
            .Argument<IntGraphType>("limit")
            .Argument<StringGraphType>("search")
            .Argument<StringGraphType>("filteringStatus")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var offset = context.GetArgument<int?>("offset");
                var limit = context.GetArgument<int?>("limit");
                var search = context.GetArgument<string>("search");
                var filteringStatus = context.GetArgument<string?>("filteringStatus");

                var usersBusinessResponse = await service.GetAllUsersAsync(offset, limit, search,
                    filteringEmploymentRate: null, filteringStatus, sortingColumn: null);

                var usersResponse = mapper.Map<PaginationResponse<ProfileResponse>>(usersBusinessResponse);
                return usersResponse;
            });
    }
}