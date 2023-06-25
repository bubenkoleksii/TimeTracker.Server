using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication;
using TimeTracker.Server.GraphQL.GraphQLTypes;
using TimeTracker.Server.Repository.Interfaces;

namespace TimeTracker.Server.GraphQL.GraphQLQueries
{
    public class UserQuery : ObjectGraphType
    {
        private readonly IUserRepository _repo;

        public UserQuery(IUserRepository repo)
        {
            _repo = repo;

            Field<UserType>("user")
                .Argument<NonNullGraphType<IntGraphType>>("id")
                .ResolveAsync(async context =>
                {
                    int id = context.GetArgument<int>("id");
                    var user = await _repo.GetUserAsync(id);
                    return await _repo.GetUserAsync(id);
                }).AuthorizeWithPolicy("Authenticated"); ;
        }
    }
}