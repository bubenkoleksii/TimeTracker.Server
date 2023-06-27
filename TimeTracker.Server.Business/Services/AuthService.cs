using AutoMapper;
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

    public AuthService(IJwtService jwtService, IUserRepository userRepository, IMapper mapper)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<AuthBusinessResponse> Login(AuthBusinessRequest userRequest)
    {
        try
        {
            var user = await _userRepository.GetUserByEmail(userRequest.Email);
            if (user == null)
                throw new ArgumentNullException($"User with email {userRequest.Email} not found");

            if (!IsPasswordValid(userRequest.Password, user.HashPassword))
                throw new AuthenticationException($"Password {userRequest.Password} is wrong");

            var userClaims = _mapper.Map<AuthTokenClaimsModel>(userRequest);
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

    public async Task Logout(Guid id)
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

    public async Task<AuthBusinessResponse> RefreshTokens(string email, string refreshToken)
    {
        try
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
                throw new ArgumentNullException($"User with email {email} not found");

            if (user.RefreshToken != refreshToken)
                throw new AuthorizationException("Refresh token is wrong");

            var userClaims = _mapper.Map<AuthTokenClaimsModel>(user);
            var newRefreshToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Refresh);
            var newAccessToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);

            await _userRepository.SetRefreshToken(refreshToken, user.Id);

            return new AuthBusinessResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }
        catch
        {
            throw new InvalidOperationException("Failed to refresh tokens");
        }
    }

    private bool IsPasswordValid(string password, string passwordHash)
    {
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
        return isPasswordValid;
    }
}