using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Shared.Exceptions;

namespace TimeTracker.Server.Data;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnectionString");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidConfigurationException("Connection string cannot be null or empty");

        _connectionString = connectionString;
    }

    public IDbConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}