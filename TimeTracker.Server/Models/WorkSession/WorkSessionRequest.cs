namespace TimeTracker.Server.Models.WorkSession;

public record WorkSessionRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }
}