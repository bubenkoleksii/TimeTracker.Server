using TimeTracker.Server.Models.User;

namespace TimeTracker.Server.Models.WorkSession;

public class WorkSessionWithRelationsResponse
{
    public WorkSessionResponse WorkSession { get; set; } = new WorkSessionResponse();

    public UserResponse User { get; set; } = new UserResponse();

    public UserResponse LastModifier { get; set; } = new UserResponse();
}