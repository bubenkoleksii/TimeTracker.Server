using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
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
            .Argument<NonNullGraphType<CreateUserInputType>>("user")
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


        // Only for admin
        Field<bool>("addSetPasswordLink")
            .Argument<NonNullGraphType<StringGraphType>>("email")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var email = context.GetArgument<string>("email");

                await service.AddSetPasswordLink(email);

                return true;
            });

        Field<bool>("setPassword")
            .Argument<NonNullGraphType<SetPasswordUserInputType>>("user")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var userRequest = context.GetArgument<SetPasswordUserRequest>("user");

                var userBusinessRequest = mapper.Map<SetPasswordUserBusinessRequest>(userRequest);
                await service.SetPassword(userBusinessRequest);

                return true;
            });

        // TODO: Reset password
    }
}