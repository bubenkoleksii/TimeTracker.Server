namespace TimeTracker.Server.Models.User;

public record UserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public string FullName { get; set; } = null!;

    public int EmploymentRate { get; set; }

    public string? Permissions { get; set; } = null!;

    public string Status { get; set; } = null!;
}