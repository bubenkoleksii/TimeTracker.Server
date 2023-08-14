namespace TimeTracker.Server.Data.Models.Vacation;

public class VacationApproveDataRequest
{
    public Guid Id { get; set; }

    public bool IsApproved { get; set; }

    public Guid ApproverId { get; set; }

    public string? ApproverComment { get; set; }
}