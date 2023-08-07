using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;

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

        foreach (var user in users)
        {
            var plannedWorkSession = new WorkSessionDataRequest()
            {
                UserId = user.Id,
                Start = workSessionStart,
                End = workSessionEnd,
                Type = "Auto"
            };
            await _workSessionRepository.CreateWorkSessionAsync(plannedWorkSession);
        }
    }
}