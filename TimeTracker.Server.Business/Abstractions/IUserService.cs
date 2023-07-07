using TimeTracker.Server.Business.Models.User;

namespace TimeTracker.Server.Business.Abstractions;

public interface IUserService
{
    public Task<UserBusinessResponse> CreateUser(UserBusinessRequest userRequest);

    public Task AddSetPasswordLink(string email);

    public Task SetPassword(SetPasswordUserBusinessRequest userRequest);
}