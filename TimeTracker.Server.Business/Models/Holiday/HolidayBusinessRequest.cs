namespace TimeTracker.Server.Business.Models.Holiday;

public class HolidayBusinessRequest
{
    public string Title { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
}