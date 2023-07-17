namespace TimeTracker.Server.Data.Models.User;

public record UserDataRequest
{
    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int EmploymentRate { get; set; }

    public string? Permissions { get; set; } = null!;

    public string Status { get; set; } = null!;
}