namespace TimeTracker.Server.Models.Holiday;

public class HolidayRequest
{
    public string Title { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
}