using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<UserDataResponse> GetUserById(Guid id)
    {
        var query = $"SELECT * FROM [User] WHERE {nameof(UserDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        var user = await connection.QuerySingleOrDefaultAsync<UserDataResponse>(query, new { id });

        return user;
    }

    public async Task<UserDataResponse> GetUserByEmail(string email)
    {
        var query = $"SELECT * FROM [User] WHERE {nameof(UserDataResponse.Email)} = @{nameof(email)}";

        using var connection = _context.GetConnection();
        var user = await connection.QuerySingleOrDefaultAsync<UserDataResponse>(query, new { email });

        return user;
    }

    public async Task<UserDataResponse> CreateUser(UserDataRequest userRequest)
    {
        var id = Guid.NewGuid();

        var queryString = $"INSERT INTO [User] (Id, {nameof(UserDataRequest.Email)}, {nameof(UserDataRequest.HashPassword)}) " +
                          $"VALUES(@{nameof(id)}, @{nameof(userRequest.Email)}, @{nameof(userRequest.HashPassword)})";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(queryString, new
        {
            id, 
            userRequest.Email, 
            userRequest.HashPassword
        });

        var userResponse = await GetUserById(id);
        if (userResponse == null)
            throw new InvalidOperationException("User has not been added");

        return userResponse;
    }

    public async Task SetRefreshToken(string refreshToken, Guid id)
    {
        var query = $"UPDATE [User] SET {nameof(UserDataResponse.RefreshToken)} = @{nameof(refreshToken)} " +
                    $"WHERE {nameof(UserDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { refreshToken, id });
    }

    public async Task RemoveRefresh(Guid id)
    {
        var query = $"UPDATE [User] SET {nameof(UserDataResponse.RefreshToken)} = NULL WHERE {nameof(UserDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { id });
    }
}