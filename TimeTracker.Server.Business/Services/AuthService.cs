using AutoMapper;
using GraphQL;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Shared.Exceptions;

namespace TimeTracker.Server.Business.Services;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;

    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IJwtService jwtService, IUserRepository userRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthBusinessResponse> LoginAsync(AuthBusinessRequest userRequest)
    {
        try
        {
            var user = await _userRepository.GetUserByEmail(userRequest.Email);
            if (user == null)
                throw new ArgumentNullException($"User with email {userRequest.Email} not found");

            if (!IsPasswordValid(userRequest.Password, user.HashPassword))
                throw new AuthenticationException($"Password {userRequest.Password} is wrong");

            var userClaims = _mapper.Map<AuthTokenClaimsModel>(userRequest);
            userClaims.Id = user.Id;
            var refreshToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Refresh);
            var accessToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);

            await _userRepository.SetRefreshToken(refreshToken, user.Id);

            return new AuthBusinessResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }
        catch
        {
            throw new InvalidOperationException("Login failed");
        }
    }

    public string GetAccessToken()
    {
        try
        {
            if (!_httpContextAccessor.HttpContext!.Request.Headers.TryGetValue("Authorization", out var accessToken))
            {
                throw new ExecutionError("You need to be authorized to run this query");
            }
            var accessTokenStr = accessToken.ToString();
            if (!accessTokenStr.StartsWith("Bearer "))
            {
                throw new ExecutionError("Access denied");
            }
            return accessTokenStr.Replace("Bearer ", "");
        }
        catch
        {
            throw new ExecutionError("Operation failed");
        }
    }

    public IEnumerable<Claim> GetUserClaims(string jwtToken)
    {
        try
        {
            JwtSecurityToken decodedToken = new JwtSecurityToken(jwtToken);
            return decodedToken.Payload.Claims;
        }
        catch
        {
            throw new ExecutionError("Operation failed");
        }
    }

    public string? GetClaimValue(IEnumerable<Claim> claims, string key)
    {
        return claims.Where(c => c.Type == key).Select(c => c.Value).SingleOrDefault();
    }

    public async Task<bool> CheckUserAuthorizationAsync(IEnumerable<Claim> claims)
    {
        var userId = GetClaimValue(claims, "Id");
        var exp = GetClaimValue(claims, "exp");
        if (userId is null || exp is null)
        {
            throw new ExecutionError("Token is invalid");
        }

        var expLong = long.Parse(exp);
        var tokenDate = DateTimeOffset.FromUnixTimeSeconds(expLong).UtcDateTime;
        var now = DateTime.Now.ToUniversalTime();

        if (tokenDate < now)
        {
            throw new ExecutionError("Access token is expired");
        }

        var user = await _userRepository.GetUserById(Guid.Parse(userId));
        if (user is null)
        {
            throw new ExecutionError("There is no user with this ID");
        }
        return true;
    }

    public async Task LogoutAsync(Guid id)
    {
        try
        {
            await _userRepository.RemoveRefresh(id);
        }
        catch
        {
            throw new InvalidOperationException("Logout failed");
        }
    }

    public async Task<AuthBusinessResponse> RefreshTokensAsync(string email, string refreshToken)
    {
        try
        {
            var user = await _userRepository.GetUserByEmail(email);

            if (user.RefreshToken != refreshToken)
                throw new ExecutionError("Refresh token is wrong");

            var userClaims = _mapper.Map<AuthTokenClaimsModel>(user);
            var newRefreshToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Refresh);
            var newAccessToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);

            await _userRepository.SetRefreshToken(newRefreshToken, user.Id);

            return new AuthBusinessResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }
        catch
        {
            throw new ExecutionError("Failed to refresh tokens");
        }
    }

    private bool IsPasswordValid(string password, string passwordHash)
    {
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
        return isPasswordValid;
    }
}