﻿using AutoMapper;
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

    public async Task<AuthBusinessResponse> LoginAsync(AuthBusinessRequest userRequest)
    {
        try
        {
            var user = await _userRepository.GetUserByEmail(userRequest.Email) ?? throw new Exception();

            if (!IsPasswordValid(userRequest.Password, user.HashPassword))
                throw new Exception();

            var userClaims = _mapper.Map<AuthTokenClaimsModel>(userRequest);
            userClaims.Id = user.Id;

            var refreshToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Refresh);
            var accessToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);

            await _userRepository.SetRefreshToken(refreshToken, user.Id);

            _httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            });

            return new AuthBusinessResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
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
        var jwt = _jwtService.GetAccessToken();
        var claims = _jwtService.GetUserClaims(jwt);
        await _jwtService.RequireUserAuthorizationAsync(claims);

        var userId = _jwtService.GetClaimValue(claims, "Id");

        await _userRepository.RemoveRefresh(Guid.Parse(userId));
    }

    public async Task<AuthBusinessResponse> RefreshTokensAsync()
    {
        if (!(_httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)))
        {
            var error = new ExecutionError("Refresh token not found")
            {
                Code = "OPERATION_FAILED"
            };
            throw error;
        }

        var claims = _jwtService.GetUserClaims(refreshToken);
        await _jwtService.RequireUserAuthorizationAsync(claims);
        var userId = _jwtService.GetClaimValue(claims, "Id");
        var user = await _userRepository.GetUserById(Guid.Parse(userId));

        if (user.RefreshToken != refreshToken)
        {
            var error = new ExecutionError("Failed to refresh tokens")
            {
                Code = "OPERATION_FAILED"
            };
            throw error;
        }

        var userClaims = _mapper.Map<AuthTokenClaimsModel>(user);
        //var newRefreshToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Refresh);
        var newAccessToken = _jwtService.GenerateJwtToken(userClaims, JwtTokenType.Access);

        //await _userRepository.SetRefreshToken(newRefreshToken, user.Id);

        return new AuthBusinessResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = "asd"
        };
    }

    private static bool IsPasswordValid(string password, string passwordHash)
    {
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
        return isPasswordValid;
    }
}