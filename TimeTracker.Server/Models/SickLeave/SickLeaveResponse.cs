namespace TimeTracker.Server.Models.SickLeave;

public class SickLeaveResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LastModifierId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}