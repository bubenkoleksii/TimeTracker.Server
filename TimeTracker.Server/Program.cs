using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Services;
using TimeTracker.Server.Data;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Repositories;
using TimeTracker.Server.Data.Migrations;
using TimeTracker.Server.GraphQl;
using FluentMigrator.Runner;
using TimeTracker.Server.Middleware;

namespace TimeTracker.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        builder.Services.AddGraphQL(graphQlBuilder => graphQlBuilder
            .AddSystemTextJson()
            .AddSchema<RootSchema>()
            .AddGraphTypes(typeof(RootSchema).Assembly));

        builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddSqlServer()
                .WithGlobalConnectionString(builder.Configuration["ConnectionStrings:DefaultConnectionString"])
                .ScanIn(typeof(Migration_20230627112400).Assembly).For.Migrations())
            .AddLogging(loggingBuilder => loggingBuilder
                .ClearProviders()
                .AddFluentMigratorConsole());

        // DI
        builder.Services.AddSingleton<DapperContext>();

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        var app = builder.Build();

        app.UseGraphQL();
        app.UseGraphQLAltair();

        Database.EnsureDatabase(
            app.Configuration["ConnectionStrings:EnsureDatabaseConnectionString"], 
            app.Configuration["Database:Name"]
            );

        app.UseMigrations();

        app.Run();
    }
}