using GraphQL.Types;
using TimeTracker.Server.Models;

namespace TimeTracker.Server.GraphQL.GraphQLTypes
{
    public class UserType : ObjectGraphType<User>
    {
        public UserType() 
        {
            Field(user => user.id, type: typeof(IntGraphType));
            Field(user => user.login, type: typeof(StringGraphType));
            Field(user => user.password, type: typeof(StringGraphType));
            Field(user => user.refreshToken, type: typeof(StringGraphType));
        }
    }
}