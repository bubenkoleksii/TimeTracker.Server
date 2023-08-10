using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared;

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
        var users = await _userRepository.GetFullTimeUsersAsync();

        DateTime workSessionStart = DateTime.Today + new TimeSpan(5, 0, 0);
        DateTime workSessionEnd = DateTime.Today + new TimeSpan(13, 0, 0);

        var workSessionsToAutoAdd = new List<WorkSessionDataRequest>();

        foreach (var user in users)
        {
            if (user.Status == UserStatusEnum.working.ToString())
            {
                workSessionsToAutoAdd.Add(new WorkSessionDataRequest()
                {
                    UserId = user.Id,
                    Start = workSessionStart,
                    End = workSessionEnd,
                    Title = null,
                    Description = null,
                    Type = "Auto",
                    LastModifierId = user.Id
                });
            }
        }

        await _workSessionRepository.CreateWorkSessionsAsync(workSessionsToAutoAdd);
    }
}