using System.Globalization;
using Quartz;
using TimeTracker.Server.Business.Abstractions;

namespace TimeTracker.Server.Quartz.Jobs;

public class EmailNotificationAboutWorkHoursJob : IJob
{
    private readonly IMailService _mailService;

    private readonly IUserService _userService;

    public EmailNotificationAboutWorkHoursJob(IMailService mailService, IUserService userService)
    {
        _mailService = mailService;
        _userService = userService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-2);

        var usersWorkInfo = await _userService.GetAllUsersWorkInfoAsync(offset: null, limit: null, search: null, 
            filteringEmploymentRate: null, filteringStatus: null, sortingColumn: null, firstDayOfMonth, lastDayOfMonth, withoutPagination: true);

        if (usersWorkInfo.Items == null)
            return;

        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(firstDayOfMonth.Month);

        var subject = $"TimeTracker: Insufficient Work Hours for the {monthName}";

        foreach (var user in usersWorkInfo.Items)
        {
            if (user.PlannedWorkingHours <= user.WorkedHours)
                continue;

            var text = @$"
                <div>
                    <h1>Insufficient Work Hours for the {monthName}</h1>
                    <p>
                        Dear {user.FullName}, this month you have worked {user.WorkedHours} out of {user.PlannedWorkingHours} ({Math.Round(user.WorkedHours / user.PlannedWorkingHours * 100, 2)}%). 
                        You did not work {user.PlannedWorkingHours - user.WorkedHours} hours of the total.
                        Hurry up.
                    </p>
                </div>";

            await _mailService.SendTextMessageAsync(user.Email, subject, text);
        }
    }
}