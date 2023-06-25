using TimeTracker.Server.Models;

namespace TimeTracker.Server.Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> GetUserAsync(int id);
    }
}