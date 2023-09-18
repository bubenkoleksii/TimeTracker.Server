using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Data.Models.Holidays;
using TimeTracker.Server.Shared;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server.Quartz.Jobs;

public class AutoWorkSessionsJob : IJob
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHolidayRepository _holidayRepository;

    private readonly int startHour;
    private readonly int fullDayHours;
    private readonly int shortDayHours;

    public AutoWorkSessionsJob(IWorkSessionRepository workSessionRepository, IUserRepository userRepository, IHolidayRepository holidayRepository, IConfiguration configuration)
    {
        _workSessionRepository = workSessionRepository;
        _userRepository = userRepository;
        _holidayRepository = holidayRepository;

        startHour = configuration.GetValue<int>("WorkHours:UtcStartHour");
        fullDayHours = configuration.GetValue<int>("WorkHours:FullDay");
        shortDayHours = configuration.GetValue<int>("WorkHours:ShortDay");
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (WorkSessionHelper.IsNotWeekendDay(DateTime.UtcNow))
        {
            var users = await _userRepository.GetFullTimeWorkingUsersAsync();

            if (users is null || !users.Any())
            {
                return;
            }

            DateTime workSessionStart = DateTime.Today.AddHours(startHour);
            DateTime workSessionEnd = workSessionStart.AddHours(fullDayHours);

            var todayShortenedDays = await _holidayRepository.GetHolidaysByDateAsync(DateOnly.FromDateTime(DateTime.Now), HolidayTypesEnum.ShortDay);
            if (todayShortenedDays is not null && todayShortenedDays.Count > 0)
            {
                workSessionEnd = workSessionStart.AddHours(shortDayHours);
            }

            var workSessionsToAutoAdd = new List<WorkSessionDataRequest>();

            foreach (var user in users)
            {
                workSessionsToAutoAdd.Add(new WorkSessionDataRequest()
                {
                    UserId = user.Id,
                    Start = workSessionStart,
                    End = workSessionEnd,
                    Title = null,
                    Description = null,
                    Type = WorkSessionTypeEnum.Auto.ToString(),
                    LastModifierId = user.Id
                });
            }

            await _workSessionRepository.CreateWorkSessionsAsync(workSessionsToAutoAdd);
        }
    }
}