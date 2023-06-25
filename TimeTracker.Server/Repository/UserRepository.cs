using Dapper;
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

        public async Task<User> GetUserAsync(int id)
        {
            string query = "SELECT * FROM Users WHERE id = @id";

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QuerySingleOrDefaultAsync<User>(query, new { id });
                Console.WriteLine(user.id);
                return user;
            }
        }
    }
}