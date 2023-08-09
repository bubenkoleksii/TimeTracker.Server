using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.Models.Vacation;

public class VacationWithUserResponse
{
    public VacationResponse Vacation { get; set; } = new VacationResponse();

    public UserResponse User { get; set; } = new UserResponse();

    public UserResponse? Aprover { get; set; } = null;
}