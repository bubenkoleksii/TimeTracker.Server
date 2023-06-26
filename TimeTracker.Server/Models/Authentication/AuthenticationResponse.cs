namespace TimeTracker.Server.Models.Authentication
{
    public class AuthenticationResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }

        public AuthenticationResponse(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public AuthenticationResponse(string accessToken, string refreshToken, string message)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Message = message;
        }

        public AuthenticationResponse(string message)
        {
            Message = message;
        }
    }
}