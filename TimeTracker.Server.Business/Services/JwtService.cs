using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Data.Abstractions;

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

    private readonly IHttpContextAccessor _contextAccessor;

    private readonly IUserRepository _userRepository;

    public JwtService(IConfiguration configuration, IHttpContextAccessor contextAccessor, IUserRepository userRepository)
    {
        _refreshTokenKey = configuration.GetSection("Auth:RefreshTokenKey").Value;
        _accessTokenKey = configuration.GetSection("Auth:AccessTokenKey").Value;

        _refreshTokenLifeTimeSeconds = int.Parse(configuration.GetSection("Auth:RefreshTokenLifeTimeSeconds").Value);
        _accessTokenLifeTimeSeconds = int.Parse(configuration.GetSection("Auth:AccessTokenLifeTimeSeconds").Value);

        _contextAccessor = contextAccessor;
        _userRepository = userRepository;
    }

    public string GenerateJwtToken(AuthTokenClaimsModel authClaims, JwtTokenType tokenType)
    {
        var claims = new List<Claim>
        {
            new ("Id", authClaims.Id.ToString()),
            new ("Email", authClaims.Email)
        };

        var tokenKey = tokenType == JwtTokenType.Refresh ? _refreshTokenKey : _accessTokenKey;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenLifeTimeSeconds = tokenType == JwtTokenType.Refresh ? _refreshTokenLifeTimeSeconds : _accessTokenLifeTimeSeconds;
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddSeconds(tokenLifeTimeSeconds),
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }

    public string GetAccessToken()
    {
        try
        {
            if (!_contextAccessor.HttpContext!.Request.Headers.TryGetValue("Authorization", out var accessToken))
            {
                throw new Exception();
            }
            var accessTokenStr = accessToken.ToString();
            if (!accessTokenStr.StartsWith("Bearer "))
            {
                throw new Exception();
            }
            return accessTokenStr.Replace("Bearer ", "");
        }
        catch
        {
            var error = new ExecutionError("You need to be authorized to run this query")
            {
                Code = "AUTHENTICATION_REQUIRED"
            };
            throw error;
        }
    }

    public IEnumerable<Claim> GetUserClaims(string jwtToken)
    {
        try
        {
            var decodedToken = new JwtSecurityToken(jwtToken);
            return decodedToken.Payload.Claims;
        }
        catch
        {
            var error = new ExecutionError("You need to be authorized to run this query")
            {
                Code = "AUTHENTICATION_REQUIRED"
            };
            throw error;
        }
    }

    public string? GetClaimValue(IEnumerable<Claim> claims, string key)
    {
        return claims.Where(c => c.Type == key).Select(c => c.Value).SingleOrDefault();
    }

    public async Task<bool> RequireUserAuthorizationAsync(IEnumerable<Claim> claims)
    {
        try
        {
            var userId = GetClaimValue(claims, "Id");
            var exp = GetClaimValue(claims, "exp");
            if (userId is null || exp is null)
            {
                throw new Exception();
            }

            var expLong = long.Parse(exp);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(expLong).UtcDateTime;
            var now = DateTime.Now.ToUniversalTime();

            if (tokenDate < now)
            {
                throw new Exception();
            }

            var user = await _userRepository.GetUserById(Guid.Parse(userId)) ?? throw new Exception();
            return true;
        }
        catch
        {
            var error = new ExecutionError("You need to be authorized to run this query")
            {
                Code = "AUTHENTICATION_REQUIRED"
            };
            throw error;
        }
    }
}