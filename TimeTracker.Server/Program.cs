using GraphQL;
using TimeTracker.Server.Context;
using TimeTracker.Server.GraphQL.GrpahQLSchema;
using TimeTracker.Server.Repository;
using TimeTracker.Server.Repository.Interfaces;

namespace TimeTracker.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<DapperContext>();
        builder.Services.AddSingleton<IUserRepository, UserRepository>();

        builder.Services.AddGraphQL(builder => builder
            .AddSystemTextJson()
            .AddSchema<GraphQLSchema>()
            .AddGraphTypes(typeof(GraphQLSchema).Assembly)
        );

        var app = builder.Build();

        app.UseGraphQL<GraphQLSchema>();
        app.UseGraphQLAltair();

        app.MapGet("/", () => $"Hello from Ukrainian Hubka Bob! {app.Configuration["ConnectionStrings:DefaultConnectionString"]}");
        app.Run();
    }
}