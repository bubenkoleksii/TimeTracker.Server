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
    public AuthMutation(IMapper mapper, IHttpContextAccessor contextAccessor)
    {
        Field<AuthType>("login")
            .Argument<NonNullGraphType<AuthInputType>>("auth")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            {
                var auth = context.GetArgument<AuthRequest>("auth");

                var authBusinessRequest = mapper.Map<AuthBusinessRequest>(auth);

                var authBusinessResponse = await service.LoginAsync(authBusinessRequest);

                var authResponse = mapper.Map<AuthResponse>(authBusinessResponse);
                return authResponse;
            });

        Field<BooleanGraphType>("logout")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            {
                await service.LogoutAsync();
                return true;
            });

        Field<AuthType>("refresh")
            .Argument<NonNullGraphType<StringGraphType>>("refreshToken")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            {
                var refreshToken = context.GetArgument<string>("refreshToken");
                var authBusinessResponse = await service.RefreshTokensAsync(refreshToken);
                var authResponse = mapper.Map<AuthResponse>(authBusinessResponse);
                return authResponse;
            });
    }
}