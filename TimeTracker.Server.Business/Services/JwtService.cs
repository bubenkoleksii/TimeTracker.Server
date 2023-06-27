using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Auth;

namespace TimeTracker.Server.Business.Services;

public enum JwtTokenType
{
    Refresh,
    Access
}

public class JwtService : IJwtService
{
    private readonly string _refreshTokenKey;

    private readonly string _accessTokenKey;

    private readonly int _refreshTokenLifeTimeSeconds;

    private readonly int _accessTokenLifeTimeSeconds;

    public JwtService(IConfiguration configuration)
    {
        _refreshTokenKey = configuration.GetSection("Auth:RefreshTokenKey").Value;
        _accessTokenKey = configuration.GetSection("Auth:AccessTokenKey").Value;

        _refreshTokenLifeTimeSeconds = int.Parse(configuration.GetSection("Auth:RefreshTokenLifeTimeSeconds").Value);
        _accessTokenLifeTimeSeconds = int.Parse(configuration.GetSection("Auth:AccessTokenLifeTimeSeconds").Value);
    }

    public string GenerateJwtToken(AuthTokenClaimsModel authClaims, JwtTokenType tokenType)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, authClaims.Email)
        };

        var tokenKey = tokenType == JwtTokenType.Refresh ? _refreshTokenKey : _accessTokenKey;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        var tokenLifeTimeSeconds = tokenType == JwtTokenType.Refresh ? _refreshTokenLifeTimeSeconds : _accessTokenLifeTimeSeconds;
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddSeconds(tokenLifeTimeSeconds),
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}