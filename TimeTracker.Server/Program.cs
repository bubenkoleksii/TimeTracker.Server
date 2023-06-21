namespace TimeTracker.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();

        app.MapGet("/", () => $"Hello from Ukrainian Hubka Bob! {app.Configuration["ConnectionStrings:DefaultConnectionString"]}");
        app.Run();
    }
}