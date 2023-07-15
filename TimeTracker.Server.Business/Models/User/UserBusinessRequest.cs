namespace TimeTracker.Server.Business.Models.User;

public record UserBusinessRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int? EmploymentRate { get; set; }

    public DateTime EmploymentDate { get; set; }

    public string Permissions { get; set; } = null!;

    public string Status { get; set; } = null!;
}