using GraphQL.Types;

namespace TimeTracker.Server.GraphQl;

public class RootSchema : Schema
{
    public RootSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Query = serviceProvider.GetRequiredService<RootQuery>();
        Mutation = serviceProvider.GetRequiredService<RootMutation>();
    }
}