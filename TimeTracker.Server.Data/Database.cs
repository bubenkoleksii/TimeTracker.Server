using Dapper;
using Microsoft.Data.SqlClient;
using TimeTracker.Server.Shared.Exceptions;

namespace TimeTracker.Server.Data;

public static class Database
{
    public static async Task EnsureDatabase(string connectionString, string name)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidConfigurationException("Connection string cannot be null or empty");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Database name cannot be null or empty");

        await using var connection = new SqlConnection(connectionString);
        {
            connection.Open();

            var databaseExists = connection.ExecuteScalar<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.databases WHERE name = @name) THEN 1 ELSE 0 END",
                new { name });

            if (!databaseExists)
            {
                await connection.ExecuteAsync($"CREATE DATABASE {name}");
            }
        }
    }
}