using Microsoft.Extensions.Configuration;

namespace TimeTracker.Server.Shared.Helpers;

public static class WorkSessionHelper
{
    public static bool IsNotWeekendDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    public static bool IsDateInRange(DateTime date, DateTime rangeStart, DateTime rangeEnd)
    {
        return date >= rangeStart && date <= rangeEnd;
    }
}