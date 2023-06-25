using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeTracker.Server.Models;

namespace TimeTracker.Server.Services
{
    public static class JwtTokenService
    {
        private static readonly string jwtSecretKey = Environment.GetEnvironmentVariable("JwtSecretKey")!;

        public static string GenerateJwtToken(IEnumerable<Claim> claims, int tokenDurationInSeconds)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JwtTokenIssuer"),
                audience: Environment.GetEnvironmentVariable("JwtTokenAudience"),
                claims: claims,
                expires: DateTime.Now.AddSeconds(tokenDurationInSeconds),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        public static IEnumerable<Claim> GetJwtTokenClaims(User user)
        {
            return new List<Claim>
            {
                new Claim("id", user.id.ToString()),
                new Claim("login", user.login),
            };
        }

        public static string GenerateAccessToken(User user) => GenerateJwtToken(GetJwtTokenClaims(user), 120);

        public static string GenerateRefreshToken(User user) => GenerateJwtToken(GetJwtTokenClaims(user), 2592000);
    }
}