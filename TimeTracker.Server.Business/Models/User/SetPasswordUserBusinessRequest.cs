namespace TimeTracker.Server.Business.Models.User;

public record SetPasswordUserBusinessRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string SetPasswordLink { get; set; } = null!;
}