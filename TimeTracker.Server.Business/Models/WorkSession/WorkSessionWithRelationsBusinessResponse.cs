using TimeTracker.Server.Business.Models.User;

namespace TimeTracker.Server.Business.Models.WorkSession;

public class WorkSessionWithRelationsBusinessResponse
{
    public WorkSessionBusinessResponse WorkSession { get; set; } = new WorkSessionBusinessResponse();

    public UserBusinessResponse User { get; set; } = new UserBusinessResponse();

    public UserBusinessResponse LastModifier { get; set; } = new UserBusinessResponse();
}