using TimeTracker.Server.Business.Models.User;

namespace TimeTracker.Server.Business.Abstractions;

public interface IUserService
{
    public Task<UserBusinessResponse> CreateUserAsync(UserBusinessRequest userRequest);

    public Task AddSetPasswordLinkAsync(string email);

    public Task SetPasswordAsync(SetPasswordUserBusinessRequest userRequest);
}