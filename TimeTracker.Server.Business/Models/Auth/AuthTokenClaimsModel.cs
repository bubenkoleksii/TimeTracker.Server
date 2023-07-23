namespace TimeTracker.Server.Business.Models.Auth;

public record AuthTokenClaimsModel
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int EmploymentRate { get; set; }

    public DateTime EmploymentDate { get; set; }

    public bool HasPassword { get; set; }

    public bool HasValidSetPasswordLink { get; set; }

    public string? Permissions { get; set; } = null!;

    public string Status { get; set; } = null!;
}