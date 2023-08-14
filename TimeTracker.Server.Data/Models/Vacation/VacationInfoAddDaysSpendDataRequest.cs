namespace TimeTracker.Server.Data.Models.Vacation;

public class VacationInfoAddDaysSpendDataRequest
{
    public Guid UserId { get; set; }
    public int DaysSpent { get; set; }
}