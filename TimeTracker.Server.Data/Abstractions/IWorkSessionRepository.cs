using TimeTracker.Server.Data.Models.WorkSession;

namespace TimeTracker.Server.Data.Abstractions
{
    public interface IWorkSessionRepository
    {
        public Task<IEnumerable<WorkSessionDataResponse>> GetWorkSessionsByUserId(Guid userId);
        public Task<WorkSessionDataResponse> GetWorkSessionById(Guid Id);
        public Task<WorkSessionDataResponse> GetActiveWorkSessionByUserId(Guid userId);
        public Task<WorkSessionDataResponse> CreateWorkSession(WorkSessionDataRequest workSession);
        public Task SetWorkSessionEnd(Guid Id, DateTime endDateTime);
    }
}