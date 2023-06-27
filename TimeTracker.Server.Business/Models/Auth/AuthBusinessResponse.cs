namespace TimeTracker.Server.Business.Models.Auth;

public record AuthBusinessResponse
{
    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}