using GraphQL.Types;
using TimeTracker.Server.GraphQL.GraphQLMutations;
using TimeTracker.Server.GraphQL.GraphQLQueries;

namespace TimeTracker.Server.GraphQL.GrpahQLSchema
{
    public class GraphQLSchema : Schema
    {
        public GraphQLSchema(IServiceProvider provider) : base(provider) 
        {
            Query = provider.GetRequiredService<UserQuery>();
            Mutation = provider.GetRequiredService<UserMutations>();
        }
    }
}