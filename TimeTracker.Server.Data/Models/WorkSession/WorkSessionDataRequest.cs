namespace TimeTracker.Server.Data.Models.WorkSession;

public record WorkSessionDataRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }
}