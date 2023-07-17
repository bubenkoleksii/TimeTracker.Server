using System.Security.Claims;
using AutoMapper;
using GraphQL;
using Microsoft.AspNetCore.Http;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Data.Abstractions;

namespace TimeTracker.Server.Business.Services;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;

    private readonly IMapper _mapper;

    private readonly IUserRepository _userRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IJwtService jwtService, IUserRepository userRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> LoginAsync(AuthBusinessRequest userRequest)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(userRequest.Email) ?? throw new Exception();

            if (!IsPasswordValid(userRequest.Password, user.HashPassword))
                throw new Exception();

            var userClaims = _mapper.Map<AuthTokenClaimsModel>(user);
            userClaims.Id = user.Id;

            var refreshToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Refresh);
            var accessToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);

            await _userRepository.SetRefreshTokenAsync(refreshToken, user.Id);

            _httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });

            return accessToken;
        }
        catch
        {
            var error = new ExecutionError("Login failed")
            {
                Code = "LOGIN_FAILED"
            };
            throw error;
        }
    }

    public async Task LogoutAsync()
    {
        var claims = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).Claims;

        var userId = claims.FirstOrDefault(c => c.Type == "Id");
        if (userId == null)
        {
            var error = new ExecutionError("Claim user id not found")
            {
                Code = "OPERATION_FAILED"
            };
            throw error;
        }

        await _userRepository.RemoveRefreshAsync(Guid.Parse(userId.Value));
    }

    public async Task<string> RefreshTokensAsync()
    {
        if (!(_httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)))
        {
            var error = new ExecutionError("Refresh token not found")
            {
                Code = "OPERATION_FAILED"
            };
            throw error;
        }

        var claims = _jwtService.DecodeJwtToken(refreshToken);

        var userId = claims.FirstOrDefault(c => c.Type == "Id");
        if (userId == null)
        {
            var error = new ExecutionError("Claim user id not found")
            {
                Code = "OPERATION_FAILED"
            };
            throw error;
        }

        var user = await _userRepository.GetUserByIdAsync(Guid.Parse(userId.Value));

        if (user.RefreshToken != refreshToken)
        {
            var error = new ExecutionError("Failed to refresh tokens")
            {
                Code = "OPERATION_FAILED"
            };
            throw error;
        }

        var userClaims = _mapper.Map<AuthTokenClaimsModel>(user);
        var newAccessToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);

        return newAccessToken;
    }

    private static bool IsPasswordValid(string password, string passwordHash)
    {
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
        return isPasswordValid;
    }
}