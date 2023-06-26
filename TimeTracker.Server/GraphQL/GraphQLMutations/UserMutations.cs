using GraphQL;
using GraphQL.Types;
using TimeTracker.Server.GraphQL.GraphQLTypes.Authentication;
using TimeTracker.Server.Repository.Interfaces;
using TimeTracker.Server.Services;

namespace TimeTracker.Server.GraphQL.GraphQLMutations
{
    public class UserMutations : ObjectGraphType
    {
        private readonly IUserRepository _repo;
        private readonly AuthenticationService _authService;

        public UserMutations(IUserRepository repo)
        {
            _repo = repo;
            _authService = new AuthenticationService(_repo);

            Field<AuthenticationResponseType>("login")
                .Argument<NonNullGraphType<StringGraphType>>("login")
                .Argument<NonNullGraphType<StringGraphType>>("password")
                .ResolveAsync(async context =>
                {
                    string email = context.GetArgument<string>("login");
                    string password = context.GetArgument<string>("password");
                    return await _authService.Login(email, password);
                });

            Field<AuthenticationResponseType>("logout")
                .Argument<NonNullGraphType<IntGraphType>>("id")
                .ResolveAsync(async context =>
                {
                    int userId = context.GetArgument<int>("id");
                    return await _authService.Logout(userId);
                }).AuthorizeWithPolicy("Authenticated");
        }
    }
}