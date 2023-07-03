using System.Security.Claims;
using TimeTracker.Server.Business.Models.Auth;

namespace TimeTracker.Server.Business.Abstractions;

public interface IAuthService
{
    public Task<AuthBusinessResponse> LoginAsync(AuthBusinessRequest userRequest);

    public Task LogoutAsync(Guid id);

    public Task<AuthBusinessResponse> RefreshTokensAsync(string email, string refreshToken);

    public string GetAccessToken();

    public IEnumerable<Claim> GetUserClaims(string jwtToken);

    public string? GetClaimValue(IEnumerable<Claim> claims, string key);

    public Task<bool> CheckUserAuthorizationAsync(IEnumerable<Claim> claims);
}