using TimeTracker.Server.Business.Models.Auth;

namespace TimeTracker.Server.Business.Abstractions;

public interface IAuthService
{
    public Task<AuthBusinessResponse> Login(AuthBusinessRequest userRequest);

    public Task Logout(Guid id);

    public Task<AuthBusinessResponse> RefreshTokens(string email, string refreshToken);
}