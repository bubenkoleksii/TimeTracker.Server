using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Data.Models.Vacation;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.Quartz.Jobs
{
    public class VacationEndJob : IJob
    {
        private readonly IUserRepository _userRepository;
        private readonly IVacationRepository _vacationRepository;

        public VacationEndJob(IUserRepository userRepository, IVacationRepository vacationRepository)
        {
            _userRepository = userRepository;
            _vacationRepository = vacationRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var approvedNotStartedVacations = await _vacationRepository.GetNotDeclinedNotFinishedVacationsAsync();

            if (approvedNotStartedVacations is null || !approvedNotStartedVacations.Any())
            {
                return;
            }

            var vacationsToEndList = new List<VacationDataResponse>();
            foreach (var vacation in approvedNotStartedVacations)
            {
                if (vacation.End.Date == DateTime.Today)
                {
                    vacationsToEndList.Add(vacation);
                }
            }

            if (vacationsToEndList.Count > 0)
            {
                await EndVacations(vacationsToEndList);
            }
        }

        public async Task EndVacations(List<VacationDataResponse> vacations)
        {
            var usersStatusesToSet = new List<UserSetStatusDataRequest>();
            foreach (var vacation in vacations)
            {
                var user = await _userRepository.GetUserByIdAsync(vacation.UserId);
                usersStatusesToSet.Add(new UserSetStatusDataRequest()
                {
                    Id = user.Id,
                    Status = UserStatusEnum.working.ToString(),
                });
            }

            //set user status to 'working'
            await _userRepository.SetUserStatusAsync(usersStatusesToSet);
        }
    }
}