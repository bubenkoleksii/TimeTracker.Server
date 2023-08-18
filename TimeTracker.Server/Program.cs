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
using Quartz;
using TimeTracker.Server.Quartz.Jobs;
using TimeTracker.Server.Shared;

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
        builder.Services.AddScoped<IHolidayService, HolidayService>();
        builder.Services.AddScoped<IVacationService, VacationService>();

        builder.Services.AddSingleton<DapperContext>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();
        builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
        builder.Services.AddScoped<IVacationInfoRepository, VacationInfoRepository>();
        builder.Services.AddScoped<IVacationRepository, VacationRepository>();

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
            options.AddPolicy(PermissionsEnum.LoggedIn.ToString(), (a) => a.RequireAuthenticatedUser());
            options.AddPolicy(PermissionsEnum.CreateUser.ToString(), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.CreateUser.ToString())));
            options.AddPolicy(PermissionsEnum.GetUsers.ToString(), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.GetUsers.ToString())));
            options.AddPolicy(PermissionsEnum.DeactivateUser.ToString(), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.DeactivateUser.ToString())));
            options.AddPolicy(PermissionsEnum.UpdateUser.ToString(), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.UpdateUser.ToString())));
            options.AddPolicy(PermissionsEnum.ManageHolidays.ToString(), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.ManageHolidays.ToString())));
            options.AddPolicy(PermissionsEnum.ApproveVacations.ToString(), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.ApproveVacations.ToString())));
            options.AddPolicy(PermissionsEnum.GetVacations.ToString(), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.GetVacations.ToString())));
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

        builder.Services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            var AutoWorkSessionsJobKey = new JobKey("AutoWorkSessionsJob");
            q.AddJob<AutoWorkSessionsJob>(opts => opts.WithIdentity(AutoWorkSessionsJobKey));

            var VacationJobKey = new JobKey("VacationJobKey");
            q.AddJob<VacationJob>(opts => opts.WithIdentity(VacationJobKey));

            q.AddTrigger(opts => opts
                .ForJob(AutoWorkSessionsJobKey)
                .WithIdentity("AutoWorkSessionsJobTrigger")
                .WithDailyTimeIntervalSchedule(s => s
                    .WithIntervalInHours(24)
                    .OnEveryDay()
                    //starts at 00:10
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 10))
                )
            );

            q.AddTrigger(opts => opts
                .ForJob(VacationJobKey)
                .WithIdentity("VacationJobTrigger")
                .WithDailyTimeIntervalSchedule(s => s
                    .WithIntervalInHours(24)
                    .OnEveryDay()
                    //starts at 00:05
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 5))
                )
            );
        });

        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete =  true);

        builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddSqlServer()
                .WithGlobalConnectionString(builder.Configuration["ConnectionStrings:DefaultConnectionString"])
                .ScanIn(typeof(Migration_20230627112400).Assembly).For.Migrations())
            .AddLogging(loggingBuilder => loggingBuilder
                .ClearProviders()
                .AddFluentMigratorConsole());

        builder.Logging.AddConsole();

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