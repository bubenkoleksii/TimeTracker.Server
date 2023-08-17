using TimeTracker.Server.Business.Models.User;

namespace TimeTracker.Server.Business.Models.SickLeave;

public class SickLeaveWithRelationsBusinessResponse
{
    public SickLeaveBusinessResponse SickLeave { get; set; } = new SickLeaveBusinessResponse();

    public UserBusinessResponse User { get; set; } = new UserBusinessResponse();

    public UserBusinessResponse LastModifier { get; set; } = new UserBusinessResponse();
}