using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Abstractions;

public interface IUserRepository
{
    public Task<UserDataResponse> GetUserById(Guid id);

    public Task<UserDataResponse> GetUserByEmail(string email);

    public Task<UserDataResponse> CreateUser(UserDataRequest userRequest);

    public Task SetRefreshToken(string refreshToken, Guid id);

    public Task RemoveRefresh(Guid id);

    public Task AddSetPasswordLink(Guid setPasswordLink, DateTime expired, Guid id);

    public Task SetPassword(SetPasswordUserDataRequest user);
}