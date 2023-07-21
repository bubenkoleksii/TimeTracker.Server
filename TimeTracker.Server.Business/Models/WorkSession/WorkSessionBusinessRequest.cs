namespace TimeTracker.Server.Business.Models.WorkSession;

public record WorkSessionBusinessRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }
}