using Microsoft.AspNetCore.Authentication;
using TimeTracker.Server.Models.Authentication;
using TimeTracker.Server.Repository.Interfaces;

namespace TimeTracker.Server.Services
{
    public class AuthenticationService
    {
        private readonly IUserRepository _repo;

        public AuthenticationService(IUserRepository repo) 
        {
            _repo = repo;
        }

        public async Task<AuthenticationResponse> Login(string login, string password)
        {
            try
            {
                var user = await _repo.GetUserByLoginAsync(login);
                //if (!PasswordService.CompareWithHash(user.Password, password))
                if (user.password != password)
                {
                    return new AuthenticationResponse("Wrong password!");
                }
                var accessToken = JwtTokenService.GenerateAccessToken(user);
                var refreshToken = JwtTokenService.GenerateRefreshToken(user);
                await _repo.SetRefreshTokenAsync(refreshToken, user.id);
                return new AuthenticationResponse(accessToken, refreshToken, "Jwt tokens have been successfully received!");
            }
            catch (Exception exception)
            {
                return new AuthenticationResponse(exception.Message);
            }
        }

        public async Task<AuthenticationResponse> Logout(int userId)
        {
            try
            {
                await _repo.RemoveRefreshTokenAsync(userId);
                return new AuthenticationResponse("User has successfully been logged out!");
            }
            catch (Exception exception)
            {
                return new AuthenticationResponse(exception.Message);
            }
        }

        public async Task<AuthenticationResponse> Refresh(int userId, string refreshToken)
        {
            try
            {
                var user = await _repo.GetUserAsync(userId);
                if (user.refreshToken == refreshToken)
                {
                    var newAccessToken = JwtTokenService.GenerateAccessToken(user);
                    var newRefreshToken = JwtTokenService.GenerateRefreshToken(user);
                    await _repo.SetRefreshTokenAsync(refreshToken, userId);
                    return new AuthenticationResponse(newAccessToken, newRefreshToken, "Jwt tokens have been successfully refreshed!");
                }
                return new AuthenticationResponse("Refresh tokens are different!");
            }
            catch (Exception exception)
            {
                return new AuthenticationResponse(exception.Message);
            }
        }
    }
}