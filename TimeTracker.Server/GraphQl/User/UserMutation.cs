using AutoMapper;
using GraphQL;
using GraphQL.Types;
using GraphQL.MicrosoftDI;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.GraphQl.User;

public sealed class UserMutation : ObjectGraphType
{
    public UserMutation(IMapper mapper)
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