using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.Quartz.Jobs;

public class SickLeaveStartJob : IJob
{
    private readonly IUserRepository _userRepository;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    public SickLeaveStartJob(IUserRepository userRepository, ISickLeaveRepository sickLeaveRepository)
    {
        _userRepository = userRepository;
        _sickLeaveRepository = sickLeaveRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var sickLeaveData = await _sickLeaveRepository.GetSickLeavesAsync();

        var usersStatusesToSet = new List<UserSetStatusDataRequest>();
        foreach (var sickLeave in sickLeaveData)
        {
            if (sickLeave.Start.Date == DateTime.Today)
            {
                usersStatusesToSet.Add(new UserSetStatusDataRequest()
                {
                    Id = sickLeave.UserId,
                    Status = UserStatusEnum.ill.ToString(),
                });
            }
        }

        await _userRepository.SetUserStatusAsync(usersStatusesToSet);
    }
}