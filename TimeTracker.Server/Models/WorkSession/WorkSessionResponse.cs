namespace TimeTracker.Server.Models.WorkSession;

public record WorkSessionResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }
}