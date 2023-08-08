namespace TimeTracker.Server.Business.Models.Vacation;

public class VacationInfoBusinessResponse
{
    public Guid UserId { get; set; }

    public DateTime EmploymentDate { get; set; }

    public int DaysSpent { get; set; }
}