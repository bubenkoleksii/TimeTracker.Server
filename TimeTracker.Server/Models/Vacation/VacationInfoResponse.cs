namespace TimeTracker.Server.Models.Vacation;

public class VacationInfoResponse
{
    public Guid UserId { get; set; }

    public DateTime EmploymentDate { get; set; }

    public int DaysSpent { get; set; }
}