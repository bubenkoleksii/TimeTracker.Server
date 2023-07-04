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
                throw new Exception();

            if (!IsPasswordValid(userRequest.Password, user.HashPassword))
                throw new Exception();

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
            var error = new ExecutionError("Login failed");
            error.Code = "LOGIN_FAILED";
            throw error;
        }
    }

    public string GetAccessToken()
    {
        try
        {
            if (!_httpContextAccessor.HttpContext!.Request.Headers.TryGetValue("Authorization", out var accessToken))
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
            var error = new ExecutionError("You need to be authorized to run this query");
            error.Code = "AUTHENTICATION_REQUIRED";
            throw error;
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
            var error = new ExecutionError("You need to be authorized to run this query");
            error.Code = "AUTHENTICATION_REQUIRED";
            throw error;
        }
    }

    public string? GetClaimValue(IEnumerable<Claim> claims, string key)
    {
        return claims.Where(c => c.Type == key).Select(c => c.Value).SingleOrDefault();
    }

    public async Task<bool> CheckUserAuthorizationAsync(IEnumerable<Claim> claims)
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

            var user = await _userRepository.GetUserById(Guid.Parse(userId));
            if (user is null)
            {
                throw new Exception();
            }
            return true;
        }
        catch
        {
            var error = new ExecutionError("You need to be authorized to run this query");
            error.Code = "AUTHENTICATION_REQUIRED";
            throw error;
        }
    }

    public async Task LogoutAsync(Guid id)
    {
        try
        {
            await _userRepository.RemoveRefresh(id);
        }
        catch
        {
            var error = new ExecutionError("Logout failed");
            error.Code = "LOGOUT_FAILED";
            throw error;
        }
    }

    public async Task<AuthBusinessResponse> RefreshTokensAsync(string email, string refreshToken)
    {
        try
        {
            var user = await _userRepository.GetUserByEmail(email);

            if (user.RefreshToken != refreshToken)
                throw new Exception();

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
            var error = new ExecutionError("Failed to refresh tokens");
            error.Code = "REFRESH_FAILED";
            throw error;
        }
    }

    private bool IsPasswordValid(string password, string passwordHash)
    {
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
        return isPasswordValid;
    }
}