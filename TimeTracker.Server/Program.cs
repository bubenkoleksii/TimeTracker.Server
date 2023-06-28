using System.Text;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Services;
using TimeTracker.Server.Data;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Repositories;
using TimeTracker.Server.Data.Migrations;
using TimeTracker.Server.GraphQl;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Server.Middleware;
using TimeTracker.Server.Models.Auth;

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
            .AddGraphTypes(typeof(RootSchema).Assembly)
            .AddAuthorization(settings =>
            {
                settings.AddPolicy($"{PolicyType.Authenticated}", p => p.RequireAuthenticatedUser());
            }));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        builder.Configuration.GetSection("Auth:AccessTokenKey").Value
                        )),
                    ValidIssuer = builder.Configuration.GetSection("Auth:JwtTokenIssuer").Value,
                    ValidAudience = builder.Configuration.GetSection("Auth:JwtTokenAudience").Value,
                };
            });

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddSqlServer()
                .WithGlobalConnectionString(builder.Configuration["ConnectionStrings:DefaultConnectionString"])
                .ScanIn(typeof(Migration_20230627112400).Assembly).For.Migrations())
            .AddLogging(loggingBuilder => loggingBuilder
                .ClearProviders()
                .AddFluentMigratorConsole());

        // DI
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddSingleton<DapperContext>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        var app = builder.Build();

        app.UseGraphQL();
        app.UseGraphQLAltair();

        app.UseAuthentication();
        
        Database.EnsureDatabase(
            app.Configuration["ConnectionStrings:EnsureDatabaseConnectionString"], 
            app.Configuration["Database:Name"]
            );

        app.UseMigrations();

        app.Run();
    }
}