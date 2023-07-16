namespace TimeTracker.Server.Data.Models.WorkSession;

public record WorkSessionDataResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }
}