using TimeTracker.Server.Business.Models.WorkSession;

namespace TimeTracker.Server.Business.Abstractions
{
    public interface IWorkSessionService
    {
        public Task<IEnumerable<WorkSessionBusinessResponse>> GetWorkSessionsByUserId(Guid userId);
        public Task<WorkSessionBusinessResponse> GetWorkSessionById(Guid id);
        public Task<WorkSessionBusinessResponse> GetActiveWorkSessionByUserId(Guid userId);
        public Task<WorkSessionBusinessResponse> CreateWorkSessionAsync(WorkSessionBusinessRequest workSession);
        public Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime);
    }
}