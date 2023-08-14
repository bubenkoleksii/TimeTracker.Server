using TimeTracker.Server.Business.Models.User;

namespace TimeTracker.Server.Business.Models.Vacation;

public class VacationWithUserBusinessResponse
{
    public VacationBusinessResponse Vacation { get; set; } = new VacationBusinessResponse();

    public UserBusinessResponse User { get; set; } = new UserBusinessResponse();

    public UserBusinessResponse? Aprover { get; set; } = null;
}