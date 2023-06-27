namespace TimeTracker.Server.Business.Models.Auth;

public record AuthTokenClaimsModel
{
    public string Email { get; set; } = null!;
}