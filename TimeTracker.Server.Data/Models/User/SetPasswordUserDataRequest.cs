namespace TimeTracker.Server.Data.Models.User;

public class SetPasswordUserDataRequest
{
    public string Email { get; set; } = null!;

    public string HashPassword { get; set; } = null!;
}