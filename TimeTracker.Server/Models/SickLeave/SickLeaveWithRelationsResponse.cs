using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.Models.SickLeave;

public class SickLeaveWithRelationsResponse
{
    public SickLeaveResponse SickLeave { get; set; } = new SickLeaveResponse();

    public UserResponse User { get; set; } = new UserResponse();

    public UserResponse LastModifier { get; set; } = new UserResponse();
}