using Dapper;
using System.Data;
using TimeTracker.Server.Context;
using TimeTracker.Server.Models;
using TimeTracker.Server.Repository.Interfaces;

namespace TimeTracker.Server.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _context;

        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task SetRefreshTokenAsync(string refreshToken, int id)
        {
            string query = "UPDATE Users SET refreshToken = @refreshToken WHERE id = @id;";

            var parameters = new DynamicParameters();
            parameters.Add("refreshToken", refreshToken, DbType.String);
            parameters.Add("id", id, DbType.Int64);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async Task RemoveRefreshTokenAsync(int id)
        {
            string query = "UPDATE Users SET refreshToken = NULL WHERE id = @id;";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, new { id });
            }
        }

        public async Task<User> GetUserAsync(int id)
        {
            string query = "SELECT * FROM Users WHERE id = @id";

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QuerySingleOrDefaultAsync<User>(query, new { id });
                return user;
            }
        }

        public async Task<User> GetUserByLoginAsync(string login)
        {
            string query = "SELECT * FROM Users WHERE login = @login";

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QuerySingleOrDefaultAsync<User>(query, new { login });
                return user;
            }
        }
    }
}