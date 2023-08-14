namespace TimeTracker.Server.Models.Vacation;

public class VacationRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Comment { get; set; }
}