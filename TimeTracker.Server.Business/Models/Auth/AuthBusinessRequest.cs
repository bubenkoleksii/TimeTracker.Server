namespace TimeTracker.Server.Business.Models.Auth;

public record AuthBusinessRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}