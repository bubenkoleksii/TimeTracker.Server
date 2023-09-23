using System.Configuration;
using System.Security.Claims;
using AutoMapper;
using Google.Apis.Auth;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Shared.Exceptions;

namespace TimeTracker.Server.Business.Services;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;

    private readonly IMapper _mapper;

    private readonly IUserRepository _userRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IConfiguration _configuration;

    public AuthService(IJwtService jwtService, IUserRepository userRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<string> LoginAsync(AuthBusinessRequest userRequest)
    {
        var user = await _userRepository.GetUserByEmailAsync(userRequest.Email) ?? throw new ExecutionError("There is no user with this email")
        {
            Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
        };

        if (!IsPasswordValid(userRequest.Password, user.HashPassword))
            throw new ExecutionError("Invalid password") 
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_PASSWORD.ToString()
            };

            var userClaims = _mapper.Map<AuthTokenClaimsModel>(user);
            userClaims.Id = user.Id;

            await SetRefreshTokenAsync(userClaims);
            var accessToken = GetAccessToken(userClaims);

            return accessToken;
    }

    public async Task<string> GoogleLoginAsync(OAuthBusinessRequest oAuthBusinessRequest)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(oAuthBusinessRequest.Credential, new GoogleJsonWebSignature.ValidationSettings());

        var user = await _userRepository.GetUserByEmailAsync(payload.Email) ?? throw new ExecutionError("There is no user with this email")
        {
            Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
        };

        var userClaims = _mapper.Map<AuthTokenClaimsModel>(user);
        userClaims.Id = user.Id;

        await SetRefreshTokenAsync(userClaims);
        var accessToken = GetAccessToken(userClaims);

        return accessToken;
    }

    public async Task LogoutAsync()
    {
        var claims = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).Claims;

        var userId = claims.FirstOrDefault(c => c.Type == "Id");
        if (userId is not null)
        {
            await _userRepository.RemoveRefreshAsync(Guid.Parse(userId.Value));
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
        }
    }

    public async Task<string> RefreshTokensAsync()
    {
        if (!(_httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)))
        {
            var error = new ExecutionError("Refresh token not found")
            {
                Code = GraphQLCustomErrorCodesEnum.REFRESH_TOKEN_NOT_FOUND.ToString()
            };
            throw error;
        }

        var claims = _jwtService.DecodeJwtToken(refreshToken);

        var userId = claims.FirstOrDefault(c => c.Type == "Id");
        if (userId == null)
        {
            var error = new ExecutionError("Can not find user id claim in refresh token payload")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
            throw error;
        }

        var user = await _userRepository.GetUserByIdAsync(Guid.Parse(userId.Value));

        if (user.RefreshToken != refreshToken)
        {
            var error = new ExecutionError("Refresh token doesn't match saved in db")
            {
                Code = GraphQLCustomErrorCodesEnum.REFRESH_TOKEN_NOT_MATCHED.ToString()
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

    private async Task SetRefreshTokenAsync(AuthTokenClaimsModel userClaims)
    {
        var refreshToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Refresh);

        await _userRepository.SetRefreshTokenAsync(refreshToken, userClaims.Id);

        var cookieExpInSeconds = _configuration.GetSection("Auth:RefreshTokenLifeTimeSeconds").Value;
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Expires = DateTime.UtcNow.AddSeconds(cookieExpInSeconds is not null ? Convert.ToDouble(cookieExpInSeconds) : 60 * 60 * 24 * 30),
        });
    }
    
    private string GetAccessToken(AuthTokenClaimsModel userClaims)
    {
        return _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);
    }
}