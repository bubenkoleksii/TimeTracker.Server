namespace TimeTracker.Server.Models.Vacation;

public class VacationResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Comment { get; set; }

    public bool? IsApproved { get; set; }

    public Guid? ApproverId { get; set; }

    public string? ApproverComment { get; set; }
}