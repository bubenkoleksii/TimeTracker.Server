using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;

namespace TimeTracker.Server.Data.Repositories
{
    public class WorkSessionRepository : IWorkSessionRepository
    {
        private readonly DapperContext _context;

        public WorkSessionRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WorkSessionDataResponse>> GetWorkSessionsByUserId(Guid userId)
        {
            const string query = $"SELECT * FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)}";

            using var connection = _context.GetConnection();
            var workSession = await connection.QueryAsync<WorkSessionDataResponse>(query, new { userId });

            return workSession;
        }

        public async Task<WorkSessionDataResponse> GetWorkSessionById(Guid id)
        {
            const string query = $"SELECT * FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}";

            using var connection = _context.GetConnection();
            var workSession = await connection.QuerySingleOrDefaultAsync<WorkSessionDataResponse>(query, new { id });

            return workSession;
        }

        public async Task<WorkSessionDataResponse> GetActiveWorkSessionByUserId(Guid userId)
        {
            const string query = $"SELECT * FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)} AND " +
                        $"[{nameof(WorkSessionDataResponse.End)}] is NULL";

            using var connection = _context.GetConnection();
            var workSession = await connection.QuerySingleOrDefaultAsync<WorkSessionDataResponse>(query, new { userId });

            return workSession;
        }

        public async Task<WorkSessionDataResponse> CreateWorkSession(WorkSessionDataRequest workSession)
        {
            var id = Guid.NewGuid();

            const string query = $"INSERT INTO [WorkSession] (Id, {nameof(WorkSessionDataRequest.UserId)}, {nameof(WorkSessionDataRequest.Start)}) " +
                                 $"VALUES (@{nameof(id)}, @{nameof(WorkSessionDataRequest.UserId)}, @{nameof(WorkSessionDataRequest.Start)})";

            using var connection = _context.GetConnection();
            await connection.ExecuteAsync(query, new
            {
                id,
                workSession.UserId,
                workSession.Start
            });

            var workSessionResponse = await GetWorkSessionById(id);
            if (workSessionResponse is null)
            {
                throw new InvalidOperationException("User has not been added");
            }

            return workSessionResponse;
        }

        public async Task SetWorkSessionEnd(Guid id, DateTime endDateTime)
        {
            const string query = $"UPDATE [WorkSession] SET [{nameof(WorkSessionDataResponse.End)}] = @{nameof(endDateTime)} " +
                        $"WHERE {nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}";

            using var connection = _context.GetConnection();
            await connection.ExecuteAsync(query, new { endDateTime, id });
        }
    }
}