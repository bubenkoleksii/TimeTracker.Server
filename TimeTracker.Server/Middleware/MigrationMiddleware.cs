using FluentMigrator.Runner;

namespace TimeTracker.Server.Middleware;

public class MigrationMiddleware
{
    private readonly RequestDelegate _next;

    public MigrationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMigrationRunner migrationRunner)
    {
        migrationRunner.MigrateUp();

        await _next(context);
    }
}

public static class MigrationExtension
{
    public static IApplicationBuilder UseMigrations(this IApplicationBuilder builder) =>
        builder.UseMiddleware<MigrationMiddleware>();
}