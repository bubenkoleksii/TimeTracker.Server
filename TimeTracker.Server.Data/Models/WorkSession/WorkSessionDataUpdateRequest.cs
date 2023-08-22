namespace TimeTracker.Server.Data.Models.WorkSession;

public record WorkSessionDataUpdateRequest
{
    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Guid LastModifierId { get; set; }
}