using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Services;
using TimeTracker.Server.Data;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Repositories;
using TimeTracker.Server.GraphQl;

namespace TimeTracker.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Adding AutoMapper
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // DI
        builder.Services.AddSingleton<DapperContext>();

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Adding GraphQL
        builder.Services.AddGraphQL(graphQlBuilder => graphQlBuilder
            .AddSystemTextJson()
            .AddSchema<RootSchema>()
            .AddGraphTypes(typeof(RootSchema).Assembly));

        var app = builder.Build();

        app.UseGraphQL();
        app.UseGraphQLAltair();

        app.Run();
    }
}