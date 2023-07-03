namespace TimeTracker.Server.Business.Models.Auth;

public record AuthTokenClaimsModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
}