using Quartz;
using TimeTracker.Server.Business.Abstractions;

namespace TimeTracker.Server.Quartz.Triggers;

public static class EmailNotificationAboutWorHoursTrigger
{
    public static async Task<List<ITrigger>> GetLastDaysOfMonthTriggers(IHolidayService holidayService, JobKey jobKey, int limitYear, int limitMonth = 12)
    {
        var lastDaysOFMonths = await holidayService.GetLastDaysOfMonth(limitYear, limitMonth);

        return lastDaysOFMonths.Select(day => TriggerBuilder.Create()
                .WithIdentity($"EmailNotificationAboutWorkHoursJobTrigger_{day}")
                .ForJob(jobKey)
                .StartAt(day)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1)
                    .WithRepeatCount(0))
                .Build())
            .ToList();
    }
}