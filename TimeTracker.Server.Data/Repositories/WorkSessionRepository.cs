using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.WorkSession;

namespace TimeTracker.Server.Data.Repositories;

public class WorkSessionRepository : IWorkSessionRepository
{
    private readonly DapperContext _context;

    public WorkSessionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<PaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserId(Guid userId, bool? orderByDesc,
        int offset, int limit, DateTime? filterDate)
    {
        var query = $"SELECT * FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)}";
        if (filterDate is not null)
        {
            query += $" AND DATEDIFF(DAY, [{nameof(WorkSessionDataResponse.Start)}], @{nameof(filterDate)}) = 0";
        }
        query += $" ORDER BY [{nameof(WorkSessionDataResponse.Start)}] {(orderByDesc is true ? "desc" : "")}" + 
                 $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY;";
        query += "SELECT COUNT(*) from [WorkSession];";

        using var connection = _context.GetConnection();
        await using var multiQuery = await connection.QueryMultipleAsync(query, new
        {
            userId,
            filterDate
        });
            
        var workSessions = await multiQuery.ReadAsync<WorkSessionDataResponse>();
        var count = await multiQuery.ReadSingleAsync<int>();

        var workSessionPaginationDataResponse = new PaginationDataResponse<WorkSessionDataResponse>()
        {
            Count = count,
            Items = workSessions
        };

        return workSessionPaginationDataResponse;
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

        var query =
            $"INSERT INTO [WorkSession] (Id, {nameof(WorkSessionDataRequest.UserId)}, {nameof(WorkSessionDataRequest.Start)}, [{nameof(WorkSessionDataRequest.End)}], {nameof(WorkSessionDataRequest.Type)}, {nameof(WorkSessionDataRequest.Title)}, {nameof(WorkSessionDataRequest.Description)}) " +
            $"VALUES (@{nameof(id)}, " +
            $"@{nameof(WorkSessionDataRequest.UserId)}, " +
            $"@{nameof(WorkSessionDataRequest.Start)}, " +
            $"{(workSession.End != null ? $"@{nameof(WorkSessionDataRequest.End)}" : "NULL")}, " +
            $"@{nameof(WorkSessionDataRequest.Type)}, " +
            $"{(workSession.Title != null ? $"@{nameof(WorkSessionDataRequest.Title)}" : "NULL")}, " +
            $"{(workSession.Description != null ? $"@{nameof(WorkSessionDataRequest.Description)}" : "NULL")})";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            id,
            workSession.UserId,
            workSession.Start,
            workSession.End,
            workSession.Type,
            workSession.Title,
            workSession.Description
        });

        var workSessionResponse = await GetWorkSessionById(id);
        if (workSessionResponse is null)
        {
            throw new InvalidOperationException("Work session has not been added");
        }

        return workSessionResponse;
    }

    public async Task SetWorkSessionEnd(Guid id, DateTime endDateTime)
    {
        const string query = $"UPDATE [WorkSession] SET [{nameof(WorkSessionDataResponse.End)}] = @{nameof(endDateTime)}," +
                             $" {nameof(WorkSessionDataResponse.Type)} = 'completed'" +
                             $" WHERE {nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { endDateTime, id });
    }

    public async Task UpdateWorkSession(Guid id, WorkSessionDataUpdateRequest workSession)
    {
        var query = "UPDATE [WorkSession] " +
                    $"SET [{nameof(WorkSessionDataResponse.Start)}] = @{nameof(workSession.Start)}, " +
                    $"[{nameof(WorkSessionDataResponse.Title)}] = {(workSession.Title != null ? $"@{nameof(workSession.Title)}" : "NULL")}, " +
                    $"[{nameof(WorkSessionDataResponse.Description)}] = {(workSession.Description != null ? $"@{nameof(workSession.Description)}" : "NULL")}, " +
                    $"[{nameof(WorkSessionDataResponse.End)}] = {(workSession.End != null ? $"@{nameof(workSession.End)}" : "NULL")} " +
                    $"WHERE [{nameof(WorkSessionDataResponse.Id)}] = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new
        {
            workSession.Start, 
            workSession.End, 
            workSession.Title, 
            workSession.Description,
            id
        });
    }
}