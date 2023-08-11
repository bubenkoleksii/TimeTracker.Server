namespace TimeTracker.Server.Models.User;

public record ProfileResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Status { get; set; } = null!;
}