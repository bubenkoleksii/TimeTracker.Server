using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Data.Models.Vacation;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.Quartz.Jobs
{
    public class VacationStartJob : IJob
    {
        private readonly IUserRepository _userRepository;
        private readonly IVacationRepository _vacationRepository;

        public VacationStartJob(IUserRepository userRepository, IVacationRepository vacationRepository)
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

            var vacationsToStartList = new List<VacationDataResponse>();
            var vacationsToDecline = new List<VacationDataResponse>();
            foreach (var vacation in approvedNotStartedVacations)
            {
                if (vacation.Start.Date == DateTime.Today)
                {
                    if (vacation.IsApproved is null)
                    {
                        vacationsToDecline.Add(vacation);
                    }
                    else if ((bool)vacation.IsApproved)
                    {
                        vacationsToStartList.Add(vacation);
                    }
                }
            }

            if (vacationsToStartList.Count > 0)
            {
                await StartVacations(vacationsToStartList);
            }

            if (vacationsToDecline.Count > 0)
            {
                await DeclineVacations(vacationsToDecline);
            }
        }

        public async Task StartVacations(List<VacationDataResponse> vacations)
        {
            var usersStatusesToSet = new List<UserSetStatusDataRequest>();

            foreach (var vacation in vacations)
            {
                var user = await _userRepository.GetUserByIdAsync(vacation.UserId);
                usersStatusesToSet.Add(new UserSetStatusDataRequest()
                {
                    Id = user.Id,
                    Status = UserStatusEnum.vacation.ToString(),
                });

            }

            //set user status to 'vacation'
            await _userRepository.SetUserStatusAsync(usersStatusesToSet);
        }

        public async Task DeclineVacations(List<VacationDataResponse> vacations)
        {
            await _vacationRepository.DeclineVacationsAsync(vacations);
        }
    }
}