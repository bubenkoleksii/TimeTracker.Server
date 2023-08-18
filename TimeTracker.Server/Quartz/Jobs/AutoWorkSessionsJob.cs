﻿using Quartz;
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
        var users = await _userRepository.GetFullTimeWorkingUsersAsync();

        DateTime workSessionStart = DateTime.Today + new TimeSpan(5, 0, 0);
        DateTime workSessionEnd = DateTime.Today + new TimeSpan(13, 0, 0);

        var workSessionsToAutoAdd = new List<WorkSessionDataRequest>();

        foreach (var user in users)
        {
            if (WorkSessionHelper.IsNotWeekendDay(workSessionStart))
            {
                workSessionsToAutoAdd.Add(new WorkSessionDataRequest()
                {
                    UserId = user.Id,
                    Start = workSessionStart,
                    End = workSessionEnd,
                    Title = null,
                    Description = null,
                    Type = WorkSessionStatusEnum.Auto.ToString(),
                    LastModifierId = user.Id
                });
            }
        }

        await _workSessionRepository.CreateWorkSessionsAsync(workSessionsToAutoAdd);
    }
}