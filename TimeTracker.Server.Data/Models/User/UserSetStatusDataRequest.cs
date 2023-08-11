namespace TimeTracker.Server.Data.Models.User;

public class UserSetStatusDataRequest
{
    public Guid Id { get; set; }

    public string Status { get; set; } = string.Empty;
}