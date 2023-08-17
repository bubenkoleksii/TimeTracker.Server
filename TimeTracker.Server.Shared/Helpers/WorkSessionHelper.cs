namespace TimeTracker.Server.Shared.Helpers;

public static class WorkSessionHelper
{
    const int workDayDefaultStartHour = 5;
    const int workDayDefaultHoursToWork = 8;

    public static DateTime GetDefaultWorkSessionStart()
    {
        return DateTime.Today + new TimeSpan(workDayDefaultStartHour, 0, 0);
    }

    public static DateTime GetDefaultWorkSessionEnd(int employmentRate)
    {
        double endTimeToWorkInMinutes = (workDayDefaultStartHour * 60) + ((employmentRate / 100.0) * (workDayDefaultHoursToWork * 60));
        return DateTime.Today + new TimeSpan((long)endTimeToWorkInMinutes * TimeSpan.TicksPerMinute);
    }
}