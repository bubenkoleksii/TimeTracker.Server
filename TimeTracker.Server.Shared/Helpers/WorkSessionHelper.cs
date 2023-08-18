namespace TimeTracker.Server.Shared.Helpers;

public static class WorkSessionHelper
{
    const int workDayDefaultStartHour = 5;
    const int workDayDefaultHoursToWork = 8;

    public static DateTime GetDefaultWorkSessionStart(DateTime? start = null)
    {
        DateTime startDate = start is not null ? (DateTime)start : DateTime.Today;
        return startDate + new TimeSpan(workDayDefaultStartHour, 0, 0);
    }

    public static DateTime GetDefaultWorkSessionEnd(int employmentRate, DateTime? start = null)
    {
        DateTime startDate = start is not null ? (DateTime)start : DateTime.Today;
        double endTimeToWorkInMinutes = (workDayDefaultStartHour * 60) + ((employmentRate / 100.0) * (workDayDefaultHoursToWork * 60));
        return startDate + new TimeSpan((long)endTimeToWorkInMinutes * TimeSpan.TicksPerMinute);
    }

    public static bool IsWeekendDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    public static bool IsDateInRange(DateTime date, DateTime rangeStart, DateTime rangeEnd)
    {
        return date >= rangeStart && date <= rangeEnd;
    }
}