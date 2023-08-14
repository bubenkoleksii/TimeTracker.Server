namespace TimeTracker.Server.Data.Models.Vacation;

public class VacationDataRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Comment { get; set; }
}