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
        // Only for admin
        Field<UserType>("create")
            .Argument<NonNullGraphType<CreateUserInputType>>("user")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var user = context.GetArgument<UserRequest>("user");

                var userBusinessRequest = mapper.Map<UserBusinessRequest>(user);

                var userBusinessResponse = await service.CreateUserAsync(userBusinessRequest);

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

                await service.AddSetPasswordLinkAsync(email);

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
                await service.SetPasswordAsync(userBusinessRequest);

                return true;
            });

        Field<bool>("resetPassword")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (_, service) =>
            {
                await service.ResetPasswordAsync();

                return true;
            }).AuthorizeWithPolicy("LoggedIn");
    }
}