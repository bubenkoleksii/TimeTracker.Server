namespace TimeTracker.Server.Business.Models.Vacation;

public class VacationApproveBusinessRequest
{
    public Guid Id { get; set; }

    public bool IsApproved { get; set; }

    public Guid ApproverId { get; set; }

    public string? ApproverComment { get; set; }
}