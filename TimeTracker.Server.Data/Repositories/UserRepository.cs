using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<UserDataResponse> GetUserByIdAsync(Guid id)
    {
        var query = $"SELECT * FROM [User] WHERE {nameof(UserDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        var user = await connection.QuerySingleOrDefaultAsync<UserDataResponse>(query, new { id });

        return user;
    }

    public async Task<UserDataResponse> GetUserByEmailAsync(string email)
    {
        var query = $"SELECT * FROM [User] WHERE {nameof(UserDataResponse.Email)} = @{nameof(email)}";

        using var connection = _context.GetConnection();
        var user = await connection.QuerySingleOrDefaultAsync<UserDataResponse>(query, new { email });

        return user;
    }

    public async Task<PaginationDataResponse<UserDataResponse>> GetAllUsersAsync(int offset, int limit, string search, int? filteringEmploymentRate, string? sortingColumn)
    {
        var query = "SELECT * FROM [User]";
        var countQuery = " SELECT COUNT(*) FROM [User]";

        if (!string.IsNullOrEmpty(search))
        {
            var searchQuery = $" WHERE (LOWER({nameof(UserDataResponse.FullName)}) LIKE '%{search}%')";

            query += searchQuery;
            countQuery += searchQuery;
        }
        else
        {
            var searchQuery = " WHERE 1=1";

            query += searchQuery;
            countQuery += searchQuery;
        }

        if (filteringEmploymentRate is > 0 and <= 100)
        {
            var filteringEmploymentRateQuery = $" AND {nameof(UserDataResponse.EmploymentRate)} = {filteringEmploymentRate}";

            query += filteringEmploymentRateQuery;
            countQuery += filteringEmploymentRateQuery;
        }

        query += " ORDER BY";
        if (sortingColumn is not null && string.Equals(sortingColumn, $"{nameof(UserDataResponse.FullName)}",
                StringComparison.CurrentCultureIgnoreCase))
        {
            query += $" {nameof(UserDataResponse.FullName)}";
        } else if (sortingColumn is not null && string.Equals(sortingColumn, $"{nameof(UserDataResponse.EmploymentDate)}",
                       StringComparison.CurrentCultureIgnoreCase))
        {
            query += $" {nameof(UserDataResponse.EmploymentDate)} DESC";
        }
        else
        {
            query += " Id";
        }

        query += $" OFFSET @{nameof(offset)} ROWS FETCH NEXT @{nameof(limit)} ROWS ONLY";

        query += ";" + countQuery;

        using var connection = _context.GetConnection();
        await using var multiQuery = await connection.QueryMultipleAsync(query, new {offset, limit});

        var users = await multiQuery.ReadAsync<UserDataResponse>();
        var count = await multiQuery.ReadSingleAsync<int>();

        if (users != null)
        {
            foreach (var user in users)
            {
                user.HasValidSetPasswordLink =
                    user.SetPasswordLink != null && user.SetPasswordLinkExpired > DateTime.UtcNow;
            }
        }

        var response = new PaginationDataResponse<UserDataResponse>
        {
            Items = users,
            Count = count
        };

        return response;
    }

    public async Task<UserDataResponse> CreateUserAsync(UserDataRequest userRequest)
    {
        var id = Guid.NewGuid();

        var queryString = userRequest.Permissions != null
            ? $"INSERT INTO [User] (Id, {nameof(UserDataRequest.Email)}, {nameof(UserDataRequest.FullName)}, {nameof(UserDataRequest.Status)}, " +
              $"{nameof(UserDataRequest.Permissions)}, {nameof(UserDataRequest.EmploymentRate)}, {nameof(UserDataRequest.EmploymentDate)}) " +
              $"VALUES (@{nameof(id)}, @{nameof(userRequest.Email)}, @{nameof(userRequest.FullName)}, @{nameof(userRequest.Status)}, " +
              $"@{nameof(userRequest.Permissions)}, @{nameof(userRequest.EmploymentRate)}, @{nameof(userRequest.EmploymentDate)})"

            : $"INSERT INTO [User] (Id, {nameof(UserDataRequest.Email)}, {nameof(UserDataRequest.FullName)}, {nameof(UserDataRequest.Status)}, " +
              $"{nameof(UserDataRequest.EmploymentRate)}, {nameof(UserDataRequest.EmploymentDate)}) " +
              $"VALUES (@{nameof(id)}, @{nameof(userRequest.Email)}, @{nameof(userRequest.FullName)}, @{nameof(userRequest.Status)}, " +
              $"@{nameof(userRequest.EmploymentRate)} @{nameof(userRequest.EmploymentDate)})";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(queryString, new
        {
            id, 
            userRequest.Email, 
            userRequest.FullName,
            userRequest.Status,
            userRequest.Permissions,
            userRequest.EmploymentRate,
            userRequest.EmploymentDate
        });

        var userResponse = await GetUserByIdAsync(id);
        if (userResponse == null)
            throw new InvalidOperationException("User has not been added");

        return userResponse;
    }

    public async Task SetRefreshTokenAsync(string refreshToken, Guid id)
    {
        var query = $"UPDATE [User] SET {nameof(UserDataResponse.RefreshToken)} = @{nameof(refreshToken)} " +
                    $"WHERE {nameof(UserDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { refreshToken, id });
    }

    public async Task RemoveRefreshAsync(Guid id)
    {
        var query = $"UPDATE [User] SET {nameof(UserDataResponse.RefreshToken)} = NULL WHERE {nameof(UserDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { id });
    }

    public async Task AddSetPasswordLinkAsync(Guid setPasswordLink, DateTime expired, Guid id)
    {
        var query = $"UPDATE [User] SET {nameof(UserDataResponse.SetPasswordLink)} = @{nameof(setPasswordLink)}, " +
                          $"{nameof(UserDataResponse.SetPasswordLinkExpired)} = @{nameof(expired)}" +
                          $" WHERE {nameof(UserDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new
        {
            setPasswordLink,
            expired,
            id
        });
    }

    public async Task SetPasswordAsync(SetPasswordUserDataRequest user)
    {
        var query = $"UPDATE [User] SET {nameof(SetPasswordUserDataRequest.HashPassword)} = @{nameof(user.HashPassword)}, {nameof(UserDataResponse.HasPassword)} = 1," +
                          $" {nameof(UserDataResponse.SetPasswordLink)} = NULL, {nameof(UserDataResponse.SetPasswordLinkExpired)} = NULL" +
                          $" WHERE Email = @{nameof(user.Email)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new
        {
           user.HashPassword,
           user.Email
        });
    }

    public async Task RemovePasswordAsync(Guid id)
    {
        var query = $"UPDATE [User] SET {nameof(SetPasswordUserDataRequest.HashPassword)} = NULL, {nameof(UserDataResponse.HasPassword)} = 0, {nameof(UserDataResponse.RefreshToken)} = NULL" +
                          $" WHERE Id = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new
        {
            id
        });
    }
}