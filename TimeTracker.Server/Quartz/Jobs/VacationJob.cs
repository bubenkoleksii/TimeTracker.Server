using Quartz;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Data.Models.Vacation;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.Quartz.Jobs
{
    public class VacationJob : IJob
    {
        private readonly IWorkSessionRepository _workSessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVacationRepository _vacationRepository;
        private readonly IVacationInfoRepository _vacationInfoRepository;

        public VacationJob(IWorkSessionRepository workSessionRepository, IUserRepository userRepository, IVacationRepository vacationRepository, 
            IVacationInfoRepository vacationInfoRepository)
        {
            _workSessionRepository = workSessionRepository;
            _userRepository = userRepository;
            _vacationRepository = vacationRepository;
            _vacationInfoRepository = vacationInfoRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var approvedNotStartedVacations = await _vacationRepository.GetApprovedNotFinishedVacationsAsync();

            var vacationsToStartList = new List<VacationDataResponse>();
            var vacationsToEndList = new List<VacationDataResponse>();
            foreach (var vacation in approvedNotStartedVacations)
            {
                if (vacation.Start.Date == DateTime.Today)
                {
                    vacationsToStartList.Add(vacation);
                }
                else if (vacation.End.Date == DateTime.Today)
                {
                    vacationsToEndList.Add(vacation);
                }
            }

            if (vacationsToStartList.Count > 0)
            {
                await StartVacations(vacationsToStartList);
            }

            if (vacationsToEndList.Count > 0)
            {
                await EndVacations(vacationsToEndList);
            }
        }

        public async Task StartVacations(List<VacationDataResponse> vacations)
        {
            const int workDayDefaultStartHour = 5;
            const int workDayDefaultHoursToWork = 8;

            var workSessionsToAutoAdd = new List<WorkSessionDataRequest>();
            var daysSpentToAdd = new List<VacationInfoAddDaysSpendDataRequest>();
            var usersStatusesToSet = new List<UserSetStatusDataRequest>();

            foreach (var vacation in vacations)
            {
                var user = await _userRepository.GetUserByIdAsync(vacation.UserId);
                usersStatusesToSet.Add(new UserSetStatusDataRequest()
                {
                    UserId = user.Id,
                    Status = UserStatusEnum.vacation.ToString(),
                });

                var vacationDurationInDays = (vacation.End - vacation.Start).TotalDays + 1;
                daysSpentToAdd.Add(new VacationInfoAddDaysSpendDataRequest()
                {
                    UserId = vacation.UserId,
                    DaysSpent = (int)vacationDurationInDays
                });

                DateTime workSessionStart = DateTime.Today + new TimeSpan(workDayDefaultStartHour, 0, 0);
                //date loss because of int type
                DateTime workSessionEnd = DateTime.Today + new TimeSpan(workDayDefaultStartHour + ((user.EmploymentRate / 100) * workDayDefaultHoursToWork), 0, 0);

                for (int i = 0; i < vacationDurationInDays; i++)
                {
                    workSessionsToAutoAdd.Add(new WorkSessionDataRequest()
                    {
                        UserId = vacation.UserId,
                        Start = workSessionStart.AddDays(i),
                        End = workSessionEnd.AddDays(i),
                        Title = null,
                        Description = null,
                        Type = "Vacation",
                        LastModifierId = user.Id
                    });
                }
            }

            //create workSessions on every vacation day
            await _workSessionRepository.CreateWorkSessionsAsync(workSessionsToAutoAdd);

            //add daysSpent into user's vacationInfo
            await _vacationInfoRepository.AddDaysSpentAsync(daysSpentToAdd);

            //set user status to 'vacation'
            await _userRepository.SetUserStatusAsync(usersStatusesToSet);
        }

        public async Task EndVacations(List<VacationDataResponse> vacations)
        {
            //set user status to 'working'
        }
    }
}