namespace TimeTracker.Server.Business.Models.Track;

public record TrackBusinessRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}