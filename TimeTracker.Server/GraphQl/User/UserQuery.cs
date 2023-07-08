using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User;

public class UserQuery : ObjectGraphType
{
    public UserQuery(IMapper mapper)
    {
        // Only for admin
        Field<ListGraphType<UserType>>("getAll")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (_, service) =>
            {
                var usersBusinessResponse = await service.GetAllUsersAsync();

                var usersResponse = mapper.Map<IEnumerable<UserResponse>>(usersBusinessResponse);
                return usersResponse;
            });
    }
}