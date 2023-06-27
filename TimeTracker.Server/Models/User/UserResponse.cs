namespace TimeTracker.Server.Models.User;

public record UserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string HashPassword { get; set; } = null!;

    public string? RefreshToken { get; set; }
}