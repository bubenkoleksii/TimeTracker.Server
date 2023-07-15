namespace TimeTracker.Server.Data.Models.User;

public class UserDataResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string HashPassword { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public string FullName { get; set; } = null!;

    public int EmploymentRate { get; set; }

    public DateTime EmploymentDate { get; set; }

    public string? Permissions { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool HasPassword { get; set; }

    public Guid? SetPasswordLink { get; set; }

    public DateTime? SetPasswordLinkExpired { get; set; }
}