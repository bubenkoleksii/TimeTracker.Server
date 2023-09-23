using TimeTracker.Server.Business.Models.Auth;

namespace TimeTracker.Server.Business.Abstractions;

public interface IAuthService
{
    public Task<string> LoginAsync(AuthBusinessRequest userRequest);

    public Task<string> GoogleLoginAsync(OAuthBusinessRequest oAuthBusinessRequest);

    public Task LogoutAsync();

    public Task<string> RefreshTokensAsync();
}