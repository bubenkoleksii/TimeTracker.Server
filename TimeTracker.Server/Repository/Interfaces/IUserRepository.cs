using TimeTracker.Server.Models;

namespace TimeTracker.Server.Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> GetUserAsync(int id);
        public Task<User> GetUserByLoginAsync(string login);
        public Task SetRefreshTokenAsync(string refreshToken, int id);
        public Task RemoveRefreshTokenAsync(int id);
    }
}