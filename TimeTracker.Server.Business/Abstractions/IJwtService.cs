using System.Security.Claims;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Business.Services;

namespace TimeTracker.Server.Business.Abstractions;

public interface IJwtService
{
    public string GenerateJwtToken(AuthTokenClaimsModel authClaims, JwtTokenType tokenType);

    public ICollection<Claim> DecodeJwtToken(string token);
}