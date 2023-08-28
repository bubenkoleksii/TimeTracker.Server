using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.User;
using TimeTracker.Server.Shared;

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

                var usersBusinessResponse = await service.GetPaginatedUsersAsync(offset, limit, search, filteringEmploymentRate, filteringStatus, sortingColumn);

                var usersResponse = mapper.Map<PaginationResponse<UserResponse>>(usersBusinessResponse);
                return usersResponse;
            }).AuthorizeWithPolicy(PermissionsEnum.GetUsers.ToString());

        Field<PaginationUserWorkInfoType>("getAllWorkInfo")
            .Argument<IntGraphType>("offset")
            .Argument<IntGraphType>("limit")
            .Argument<StringGraphType>("search")
            .Argument<StringGraphType>("sortingColumn")
            .Argument<IntGraphType>("filteringEmploymentRate")
            .Argument<StringGraphType>("filteringStatus")
            .Argument<DateTimeGraphType>("start")
            .Argument<DateTimeGraphType>("end")
            .Argument<BooleanGraphType>("withoutPagination")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
             {
                var offset = context.GetArgument<int?>("offset");
                var limit = context.GetArgument<int?>("limit");
                var search = context.GetArgument<string?>("search");
                var filteringEmploymentRate = context.GetArgument<int?>("filteringEmploymentRate");
                var filteringStatus = context.GetArgument<string?>("filteringStatus");
                var sortingColumn = context.GetArgument<string?>("sortingColumn");
                var start = context.GetArgument<DateTime?>("start");
                var end = context.GetArgument<DateTime?>("end");
                var withoutPagination = context.GetArgument<bool?>("withoutPagination");

                var usersBusinessResponse = await service.GetAllUsersWorkInfoAsync(offset, limit, search, filteringEmploymentRate, filteringStatus, sortingColumn, start, end, withoutPagination);
                
                var usersResponse = mapper.Map<PaginationResponse<UserWorkInfoResponse>>(usersBusinessResponse);
                return usersResponse;
            }).AuthorizeWithPolicy(PermissionsEnum.GetUsers.ToString());
        
        Field<ListGraphType<ByteGraphType>>("exportWorkInfoToExcel")
            .Argument<StringGraphType>("search")
            .Argument<StringGraphType>("sortingColumn")
            .Argument<IntGraphType>("filteringEmploymentRate")
            .Argument<StringGraphType>("filteringStatus")
            .Argument<DateTimeGraphType>("start")
            .Argument<DateTimeGraphType>("end")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var search = context.GetArgument<string?>("search");
                var filteringEmploymentRate = context.GetArgument<int?>("filteringEmploymentRate");
                var filteringStatus = context.GetArgument<string?>("filteringStatus");
                var sortingColumn = context.GetArgument<string?>("sortingColumn");
                var start = context.GetArgument<DateTime?>("start");
                var end = context.GetArgument<DateTime?>("end");

                var excelBytes = await service.ExportUsersWorkInfoToExcel(search, filteringEmploymentRate, filteringStatus, sortingColumn, start, end);
                return excelBytes;
            }).AuthorizeWithPolicy(PermissionsEnum.GetUsers.ToString());

       Field<ListGraphType<UserType>>("getAllWithoutPagination")
            .Argument<NonNullGraphType<BooleanGraphType>>("showFired")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var showFired = context.GetArgument<bool>("showFired");

                var usersBusinessResponse = await service.GetAllUsersAsync(showFired);

                var usersResponse = mapper.Map<IEnumerable<UserResponse>>(usersBusinessResponse);
                return usersResponse;
            }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());
            
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

                var usersBusinessResponse = await service.GetPaginatedUsersAsync(offset, limit, search,
                    filteringEmploymentRate: null, filteringStatus, sortingColumn: null);

                var usersResponse = mapper.Map<PaginationResponse<ProfileResponse>>(usersBusinessResponse);
                return usersResponse;
            });
    }
}