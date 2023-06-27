namespace TimeTracker.Server.Data.Models.User;

public class UserDataResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? RefreshToken { get; set; }
}