namespace TimeTracker.Server.Business.Models.User;

public record UserBusinessResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string HashPassword { get; set; } = null!;

    public string? RefreshToken { get; set; }
}