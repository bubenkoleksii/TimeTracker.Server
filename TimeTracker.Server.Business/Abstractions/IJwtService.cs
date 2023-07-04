using System.Security.Claims;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Business.Services;

namespace TimeTracker.Server.Business.Abstractions;

public interface IJwtService
{
    public string GenerateJwtToken(AuthTokenClaimsModel authClaims, JwtTokenType tokenType);

    public string GetAccessToken();

    public IEnumerable<Claim> GetUserClaims(string jwtToken);

    public string? GetClaimValue(IEnumerable<Claim> claims, string key);

    public Task<bool> RequireUserAuthorizationAsync(IEnumerable<Claim> claims);
}