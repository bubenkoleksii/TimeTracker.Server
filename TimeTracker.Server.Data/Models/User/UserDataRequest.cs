namespace TimeTracker.Server.Data.Models.User;

public record UserDataRequest
{
    public string Email { get; set; } = null!;

    public string HashPassword { get; set; } = null!;
}