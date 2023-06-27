namespace TimeTracker.Server.Models.Auth;

public record AuthResponse
{
    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}