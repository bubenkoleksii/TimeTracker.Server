namespace TimeTracker.Server.Models.Track;

public record TrackRequest
{
    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}