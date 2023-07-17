using System.Security.Claims;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Server.Middleware;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // DI
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IMailService, MailService>();
        builder.Services.AddScoped<IWorkSessionService, WorkSessionService>();

        builder.Services.AddSingleton<DapperContext>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();

        builder.Services.AddAuthentication(conf =>
        {
            conf.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            conf.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            conf.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Auth:AccessTokenKey").Value)),
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                RequireSignedTokens = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("LoggedIn", (a) => a.RequireAuthenticatedUser());
            options.AddPolicy("CreateUser", (a) => a.RequireAssertion(context => HasPermissionClaim(context, "CreateUser")));
        });

        builder.Services.AddGraphQL(builder => builder
            .AddSystemTextJson()
            .AddSchema<RootSchema>()
            .AddGraphTypes(typeof(RootSchema).Assembly)
            .AddAuthorizationRule()
        );

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "MyAllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });


        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddSqlServer()
                .WithGlobalConnectionString(builder.Configuration["ConnectionStrings:DefaultConnectionString"])
                .ScanIn(typeof(Migration_20230627112400).Assembly).For.Migrations())
            .AddLogging(loggingBuilder => loggingBuilder
                .ClearProviders()
                .AddFluentMigratorConsole());

        var app = builder.Build();

        app.UseCors("MyAllowSpecificOrigins");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseGraphQL();
        app.UseGraphQLAltair();

        app.UseMigrations();

        await Database.EnsureDatabaseAsync(
            app.Configuration["ConnectionStrings:EnsureDatabaseConnectionString"], 
            app.Configuration["Database:Name"]
            );

        app.Run();
    }

    private static bool HasPermissionClaim(AuthorizationHandlerContext context, string permission)
    {
        if (context.User.Identity is ClaimsIdentity claimsIdentity)
        {
            var permissionClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Permissions");

            if (permissionClaim != null)
            {
                var permissionJson = permissionClaim.Value;
                return PermissionHelper.HasPermit(permissionJson, permission);
            }
        }

        return false;
    }
}