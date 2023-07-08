namespace TimeTracker.Server.Models.Track;

public record TrackResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}