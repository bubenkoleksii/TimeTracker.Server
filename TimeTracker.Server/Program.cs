using GraphQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

        builder.Services.AddAuthentication(options => {
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidIssuer = Environment.GetEnvironmentVariable("JwtTokenIssuer"),
                ValidAudience = Environment.GetEnvironmentVariable("JwtTokenAudience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtSecretKey"))),
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddGraphQL(builder => builder
            .AddSystemTextJson()
            .AddSchema<GraphQLSchema>()
            .AddGraphTypes(typeof(GraphQLSchema).Assembly)
            .AddAuthorization(settings => settings.AddPolicy("Authenticated", p => p.RequireAuthenticatedUser()))
        );

        var app = builder.Build();

        app.UseAuthentication();
        //app.UseAuthorization();

        app.UseGraphQL<GraphQLSchema>();
        app.UseGraphQLAltair();

        app.MapGet("/", () => $"Hello from Ukrainian Hubka Bob! {app.Configuration["ConnectionStrings:DefaultConnectionString"]}");
        app.Run();
    }
}