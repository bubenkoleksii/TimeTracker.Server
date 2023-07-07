using AutoMapper;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.GraphQl.Auth.Types;
using TimeTracker.Server.Models.Auth;

namespace TimeTracker.Server.GraphQl.Auth;

public sealed class AuthMutation : ObjectGraphType
{
    public AuthMutation(IMapper mapper)
    {
        Field<string>("login")
            .Argument<NonNullGraphType<AuthInputType>>("auth")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            {
                var auth = context.GetArgument<AuthRequest>("auth");

                var authBusinessRequest = mapper.Map<AuthBusinessRequest>(auth);

                var accessToken = await service.LoginAsync(authBusinessRequest);
                return accessToken;
            });

        Field<BooleanGraphType>("logout")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            {
                await service.LogoutAsync();
                return true;
            }).AuthorizeWithPolicy("LoggedIn");

        Field<string>("refresh")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            { 
                var newAccessToken = await service.RefreshTokensAsync();
                return newAccessToken;
            }).AuthorizeWithPolicy("LoggedIn");
    }
}