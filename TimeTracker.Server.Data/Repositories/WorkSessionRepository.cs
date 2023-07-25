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

        public async Task<WorkSessionPaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool orderByDesc,
            int offset, int limit, DateTime? filterDate)
        {
            string query = $"SELECT * FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)}";
            if (filterDate is not null)
            {
                query += $" AND DATEDIFF(DAY, [{nameof(WorkSessionDataResponse.Start)}], @{nameof(filterDate)}) = 0";
            }
            query += $" ORDER BY [{nameof(WorkSessionDataResponse.Start)}] {(orderByDesc ? "desc" : "")}" + $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY;";
            query += "SELECT COUNT(*) from [WorkSession];";

            using var connection = _context.GetConnection();
            await using var multiQuery = await connection.QueryMultipleAsync(query, new
            {
                userId,
                filterDate
            });
            
            var workSessions = await multiQuery.ReadAsync<WorkSessionDataResponse>();
            var count = await multiQuery.ReadSingleAsync<int>();

            var workSessionPaginatinDataResponse = new WorkSessionPaginationDataResponse<WorkSessionDataResponse>()
            {
                Count = count,
                Items = workSessions
            };

            return workSessionPaginatinDataResponse;
        }

        public async Task<WorkSessionDataResponse> GetWorkSessionByIdAsync(Guid id)
        {
            const string query = $"SELECT * FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}";

            using var connection = _context.GetConnection();
            var workSession = await connection.QuerySingleOrDefaultAsync<WorkSessionDataResponse>(query, new { id });

            return workSession;
        }

        public async Task<WorkSessionDataResponse> GetActiveWorkSessionByUserIdAsync(Guid userId)
        {
            const string query = $"SELECT * FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)} AND " +
                        $"[{nameof(WorkSessionDataResponse.End)}] is NULL";

            using var connection = _context.GetConnection();
            var workSession = await connection.QuerySingleOrDefaultAsync<WorkSessionDataResponse>(query, new { userId });

            return workSession;
        }

        public async Task<WorkSessionDataResponse> CreateWorkSessionAsync(WorkSessionDataRequest workSession)
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

            var workSessionResponse = await GetWorkSessionByIdAsync(id);
            if (workSessionResponse is null)
            {
                throw new InvalidOperationException("User has not been added");
            }

            return workSessionResponse;
        }

        public async Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime)
        {
            const string query = $"UPDATE [WorkSession] SET [{nameof(WorkSessionDataResponse.End)}] = @{nameof(endDateTime)} " +
                        $"WHERE {nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}";

            using var connection = _context.GetConnection();
            await connection.ExecuteAsync(query, new { endDateTime, id });
        }

        public async Task UpdateWorkSessionAsync(Guid id, WorkSessionDataRequest workSession)
        {
            const string query = $"UPDATE [WorkSession] SET [{nameof(WorkSessionDataResponse.Start)}] = @{nameof(workSession.Start)}, " +
                $"[{nameof(WorkSessionDataResponse.End)}] = @{nameof(workSession.End)} WHERE [{nameof(WorkSessionDataResponse.Id)}] = @{nameof(id)}";

            using var connection = _context.GetConnection();
            await connection.ExecuteAsync(query, new { workSession.Start, workSession.End, id });
        }

        public async Task DeleteWorkSessionAsync(Guid id)
        {
            const string query = $"DELETE FROM [WorkSession] WHERE [{nameof(WorkSessionDataResponse.Id)}] = @{nameof(id)};";

            using var connection = _context.GetConnection();
            await connection.ExecuteAsync(query, new { id });
        }
    }
}