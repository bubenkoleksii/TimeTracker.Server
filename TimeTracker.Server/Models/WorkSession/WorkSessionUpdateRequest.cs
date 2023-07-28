namespace TimeTracker.Server.Models.WorkSession;

public record WorkSessionUpdateRequest
{
    public DateTime Start { get; set; }

    public DateTime? End { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }
}