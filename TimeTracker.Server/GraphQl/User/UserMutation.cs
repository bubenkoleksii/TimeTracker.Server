using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.GraphQl.User.Types;
using TimeTracker.Server.Models.User;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.GraphQl.User;

public sealed class UserMutation : ObjectGraphType
{
    public UserMutation(IMapper mapper)
    {
        Field<UserType>("create")
            .Argument<NonNullGraphType<CreateUpdateUserInputType>>("user")
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
            }).AuthorizeWithPolicy(PermissionsEnum.CreateUser.ToString());

        Field<UserType>("update")
            .Argument<NonNullGraphType<CreateUpdateUserInputType>>("user")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var user = context.GetArgument<UserRequest>("user");
                var id = context.GetArgument<Guid>("id");

                var userBusinessRequest = mapper.Map<UserBusinessRequest>(user);

                var userBusinessResponse = await service.UpdateUserAsync(userBusinessRequest, id);

                var userResponse = mapper.Map<UserResponse>(userBusinessResponse);
                return userResponse;
            }).AuthorizeWithPolicy(PermissionsEnum.UpdateUser.ToString());

        Field<bool>("deactivate")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .Resolve()
            .WithScope()
            .WithService<IUserService>()
            .ResolveAsync(async (context, service) =>
            {
                var id = context.GetArgument<Guid>("id");

                await service.DeactivateUserAsync(id);
                return true;
            }).AuthorizeWithPolicy(PermissionsEnum.DeactivateUser.ToString());

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
            }).AuthorizeWithPolicy(PermissionsEnum.CreateUser.ToString())
              .AuthorizeWithPolicy(PermissionsEnum.UpdateUser.ToString());

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
            }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());
    }
}