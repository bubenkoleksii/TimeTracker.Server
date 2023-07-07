namespace TimeTracker.Server.Data.Models.Track;

public record TrackDataRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}