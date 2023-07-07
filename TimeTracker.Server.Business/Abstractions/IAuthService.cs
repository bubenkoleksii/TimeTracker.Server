using TimeTracker.Server.Business.Models.Auth;

namespace TimeTracker.Server.Business.Abstractions;

public interface IAuthService
{
    public Task<AuthBusinessResponse> LoginAsync(AuthBusinessRequest userRequest);

    public Task LogoutAsync();

    public Task<AuthBusinessResponse> RefreshTokensAsync();
}