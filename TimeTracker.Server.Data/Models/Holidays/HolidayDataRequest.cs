namespace TimeTracker.Server.Data.Models.Holidays;

public class HolidayDataRequest
{
    public string Title { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
}