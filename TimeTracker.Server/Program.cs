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
using Microsoft.Extensions.Logging.AzureAppServices;
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
        builder.Services.AddScoped<ISickLeaveService, SickLeaveService>();

        builder.Services.AddSingleton<DapperContext>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();
        builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
        builder.Services.AddScoped<IVacationInfoRepository, VacationInfoRepository>();
        builder.Services.AddScoped<IVacationRepository, VacationRepository>();
        builder.Services.AddScoped<ISickLeaveRepository, SickLeaveRepository>();

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
            options.AddPolicy(nameof(PermissionsEnum.LoggedIn), (a) => a.RequireAuthenticatedUser());
            options.AddPolicy(nameof(PermissionsEnum.CreateUser), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.CreateUser)));
            options.AddPolicy(nameof(PermissionsEnum.GetUsers), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.GetUsers)));
            options.AddPolicy(nameof(PermissionsEnum.DeactivateUser), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.DeactivateUser)));
            options.AddPolicy(nameof(PermissionsEnum.UpdateUser), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.UpdateUser)));
            options.AddPolicy(nameof(PermissionsEnum.ManageHolidays), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.ManageHolidays)));
            options.AddPolicy(nameof(PermissionsEnum.ApproveVacations), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.ApproveVacations)));
            options.AddPolicy(nameof(PermissionsEnum.GetVacations), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.GetVacations)));
            options.AddPolicy(nameof(PermissionsEnum.ManageSickLeaves), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.ManageSickLeaves)));
            options.AddPolicy(nameof(PermissionsEnum.GetUsersWorkInfo), (a) => a.RequireAssertion(context => HasPermissionClaim(context, PermissionsEnum.GetUsersWorkInfo)));
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

            var SickLeaveStartJobKey = new JobKey("SickLeaveStartJobKey");
            q.AddJob<SickLeaveStartJob>(opts => opts.WithIdentity(SickLeaveStartJobKey));
            q.AddTrigger(opts => opts
                .ForJob(SickLeaveStartJobKey)
                .WithIdentity("SickLeaveStartJobTrigger")
                .WithDailyTimeIntervalSchedule(s => s
                    .WithIntervalInHours(24)
                    .OnEveryDay()
                    //starts at 00:05
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 5))
                )
            );

            var VacationStartJobKey = new JobKey("VacationStartJobKey");
            q.AddJob<VacationStartJob>(opts => opts.WithIdentity(VacationStartJobKey));
            q.AddTrigger(opts => opts
                .ForJob(VacationStartJobKey)
                .WithIdentity("VacationStartJobTrigger")
                .WithDailyTimeIntervalSchedule(s => s
                    .WithIntervalInHours(24)
                    .OnEveryDay()
                    //starts at 00:10
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 10))
                )
            );

            var AutoWorkSessionsJobKey = new JobKey("AutoWorkSessionsJob");
            q.AddJob<AutoWorkSessionsJob>(opts => opts.WithIdentity(AutoWorkSessionsJobKey));
            q.AddTrigger(opts => opts
                .ForJob(AutoWorkSessionsJobKey)
                .WithIdentity("AutoWorkSessionsJobTrigger")
                .WithDailyTimeIntervalSchedule(s => s
                    .WithIntervalInHours(24)
                    .OnEveryDay()
                    //starts at 00:15
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 15))
                )
            );

            
            var VacationEndJobKey = new JobKey("VacationEndJobKey");
            q.AddJob<VacationEndJob>(opts => opts.WithIdentity(VacationEndJobKey));
            q.AddTrigger(opts => opts
                .ForJob(VacationEndJobKey)
                .WithIdentity("VacationEndJobTrigger")
                .WithDailyTimeIntervalSchedule(s => s
                    .WithIntervalInHours(24)
                    .OnEveryDay()
                    //starts at 23:55
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(23, 55))
                )
            );

            var SickLeaveEndJobKey = new JobKey("SickLeaveEndJobKey");
            q.AddJob<SickLeaveEndJob>(opts => opts.WithIdentity(SickLeaveEndJobKey));
            q.AddTrigger(opts => opts
                .ForJob(SickLeaveEndJobKey)
                .WithIdentity("SickLeaveEndJobTrigger")
                .WithDailyTimeIntervalSchedule(s => s
                    .WithIntervalInHours(24)
                    .OnEveryDay()
                    //starts at 23:50
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(23, 50))
                )
            );

            var EmailNotificationAboutWorkHoursJobKey = new JobKey("EmailNotificationAboutWorkHoursJobKey");
            q.AddJob<EmailNotificationAboutWorkHoursJob>(opts => opts.WithIdentity(EmailNotificationAboutWorkHoursJobKey));

            var holidayService = builder.Services.BuildServiceProvider().GetRequiredService<IHolidayService>();
            var lastDaysOFMonths = holidayService.GetLastDaysOfMonth(2030).Result;

            foreach (var day in lastDaysOFMonths)
            {
                q.AddTrigger(opts => opts
                    .ForJob(EmailNotificationAboutWorkHoursJobKey)
                    .WithIdentity($"EmailNotificationAboutWorkHoursJobTrigger_{day}")
                    .StartAt(day) 
                    .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(1) 
                            .WithRepeatCount(0)     
                    )
                );
            }
            q.AddTrigger(opts => opts
                .ForJob(EmailNotificationAboutWorkHoursJobKey)
                .WithIdentity("EmailNotificationAboutWorkHoursJobTrigger")
                .WithCronSchedule("0 0 9 L * ? *")
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

        builder.Host.ConfigureLogging(l => l.AddAzureWebAppDiagnostics())
            .ConfigureServices(s => s.Configure<AzureFileLoggerOptions>(options =>
            {
                options.FileName = "azure-diagnostics-";
                options.FileSizeLimit = 50 * 1024;
                options.RetainedFileCountLimit = 5;
            }).Configure<AzureBlobLoggerOptions>(options =>
            {
                options.BlobName = "log.txt";
            }));

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

    private static bool HasPermissionClaim(AuthorizationHandlerContext context, PermissionsEnum permission)
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