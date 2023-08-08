namespace TimeTracker.Server.Business.Models.Vacation;

public class VacationBusinessRequest
{
    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Comment { get; set; }
}