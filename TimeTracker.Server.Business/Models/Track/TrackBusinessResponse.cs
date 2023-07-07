namespace TimeTracker.Server.Business.Models.Track;

public class TrackBusinessResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}