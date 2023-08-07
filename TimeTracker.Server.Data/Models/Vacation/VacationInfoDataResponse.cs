namespace TimeTracker.Server.Data.Models.Vacation;

public class VacationInfoDataResponse
{
    public Guid UserId { get; set; }

    public DateTime EmploymentDate { get; set; }

    public int DaysSpent { get; set; }
}