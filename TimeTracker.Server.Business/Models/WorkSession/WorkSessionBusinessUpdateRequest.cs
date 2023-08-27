namespace TimeTracker.Server.Business.Models.WorkSession;

public record WorkSessionBusinessUpdateRequest
{
    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Guid LastModifierId { get; set; }
}