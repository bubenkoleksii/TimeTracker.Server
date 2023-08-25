using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server.Quartz.Jobs;

public class AutoWorkSessionsJob : IJob
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly IUserRepository _userRepository;

    public AutoWorkSessionsJob(IWorkSessionRepository workSessionRepository, IUserRepository userRepository)
    {
        _workSessionRepository = workSessionRepository;
        _userRepository = userRepository;
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
            DateTime workSessionStart = WorkSessionHelper.GetDefaultWorkSessionStart();
            DateTime workSessionEnd = WorkSessionHelper.GetDefaultWorkSessionEnd(users.First().EmploymentRate);

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