namespace TimeTracker.Server.Business.Models.SickLeave;

public class SickLeaveBusinessResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LastModifierId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}