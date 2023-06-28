using System.Security.Claims;
using AutoMapper;
using GraphQL;
using GraphQL.Types;
using GraphQL.MicrosoftDI;
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

                var authBusinessResponse = await service.Login(authBusinessRequest);

                var authResponse = mapper.Map<AuthResponse>(authBusinessResponse);
                return authResponse;
            });

        Field<BooleanGraphType>("logout")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            {
                var id = context.GetArgument<Guid>("id");
                //var h = contextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email);
                await service.Logout(id);

                return true;
            }).AuthorizeWithPolicy($"{PolicyType.Authenticated}");

        Field<AuthType>("refresh")
            .Argument<NonNullGraphType<StringGraphType>>("refreshToken")
            .Resolve()
            .WithScope()
            .WithService<IAuthService>()
            .ResolveAsync(async (context, service) =>
            {
                var refreshToken = context.GetArgument<string>("refreshToken");
                //var email = contextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Email);
                var email = "john@example8.com";

                var authBusinessResponse = await service.RefreshTokens(email, refreshToken);

                var authResponse = mapper.Map<AuthResponse>(authBusinessResponse);
                return authResponse;
            });
    }
}