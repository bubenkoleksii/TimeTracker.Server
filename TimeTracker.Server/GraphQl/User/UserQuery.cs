using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.User;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server.GraphQl.User;

public class UserQuery : ObjectGraphType
{
    public UserQuery(IMapper mapper)
    {
        Field<ListGraphType<UserType>>("getAll")
            .Argument<IntGraphType>("offset")
            .Argument<IntGraphType>("limit")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var offset = context.GetArgument<int?>("offset");
                var limit = context.GetArgument<int?>("limit");

                var usersBusinessResponse = await service.GetAllUsersAsync(offset, limit);

                var usersResponse = mapper.Map<IEnumerable<UserResponse>>(usersBusinessResponse);
                return usersResponse;
            }).AuthorizeWithPolicy("LoggedIn");
    }
}