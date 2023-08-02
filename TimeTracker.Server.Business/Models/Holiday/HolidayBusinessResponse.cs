namespace TimeTracker.Server.Business.Models.Holiday;

public class HolidayBusinessResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
}