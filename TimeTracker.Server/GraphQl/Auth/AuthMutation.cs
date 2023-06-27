using AutoMapper;
using GraphQL.Types;

namespace TimeTracker.Server.GraphQl.Auth;

public class AuthMutation : ObjectGraphType
{
    public AuthMutation(IMapper mapper)
    {
        Field<UserType>("create")
            .Argument<NonNullGraphType<UserInputType>>("user")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var user = context.GetArgument<UserRequest>("user");

                var userBusinessRequest = mapper.Map<UserBusinessRequest>(user);

                var userBusinessResponse = await service.CreateUser(userBusinessRequest);

                var userResponse = mapper.Map<UserResponse>(userBusinessResponse);
                return userResponse;
            });
    }
}