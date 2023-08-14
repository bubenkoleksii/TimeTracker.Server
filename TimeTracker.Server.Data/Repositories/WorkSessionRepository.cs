using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Data.Models.WorkSession;

namespace TimeTracker.Server.Data.Repositories;

public class WorkSessionRepository : IWorkSessionRepository
{
    private readonly DapperContext _context;

    public WorkSessionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<PaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc,
        int offset, int limit, DateTime? startDate, DateTime? endDate)
    {
        var query = $@"
            SELECT ws.{nameof(WorkSessionDataResponse.Id)}
                  ,{nameof(WorkSessionDataResponse.UserId)}
                  ,{nameof(WorkSessionDataResponse.Start)}
                  ,[{nameof(WorkSessionDataResponse.End)}]
                  ,{nameof(WorkSessionDataResponse.Type)}
                  ,{nameof(WorkSessionDataResponse.Title)}
                  ,{nameof(WorkSessionDataResponse.Description)}
                  ,{nameof(WorkSessionDataResponse.LastModifierId)}
                  ,u.{nameof(UserDataResponse.FullName)} AS {nameof(WorkSessionDataResponse.LastModifierName)}
            FROM [TimeTrackerDb].[dbo].[WorkSession] ws
            INNER JOIN [TimeTrackerDb].[dbo].[User] u ON ws.{nameof(WorkSessionDataResponse.LastModifierId)} = u.{nameof(UserDataResponse.Id)}
            WHERE ws.{nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)}
        ";
        var countQuery = $"SELECT COUNT(*) FROM [WorkSession] WHERE {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)}";

        if (startDate is not null)
        {
            var startDateQuery = $" AND {nameof(WorkSessionDataResponse.Start)} >= @{nameof(startDate)}";

            query += startDateQuery;
            countQuery += startDateQuery;
        }

        if (endDate is not null)
        {
            var endDateQuery = $" AND {nameof(WorkSessionDataResponse.Start)} <= DATEADD(day, 1, @{nameof(endDate)})";

            query += endDateQuery;
            countQuery += endDateQuery;
        }

        var withoutPlannedQuery = $" AND {nameof(WorkSessionDataResponse.Type)} != 'planned'";
        query += withoutPlannedQuery;
        countQuery += withoutPlannedQuery;

        query += $" ORDER BY [{nameof(WorkSessionDataResponse.Start)}] {(orderByDesc is true ? "desc" : "")}" + 
                 $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY;";

        query += countQuery;

        using var connection = _context.GetConnection();
        await using var multiQuery = await connection.QueryMultipleAsync(query, new
        {
            userId,
            startDate,
            endDate
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

    public async Task<WorkSessionDataResponse> GetWorkSessionByIdAsync(Guid id)
    {
        var query = $@"
            SELECT ws.{nameof(WorkSessionDataResponse.Id)}
                  ,{nameof(WorkSessionDataResponse.UserId)}
                  ,{nameof(WorkSessionDataResponse.Start)}
                  ,[{nameof(WorkSessionDataResponse.End)}]
                  ,{nameof(WorkSessionDataResponse.Type)}
                  ,{nameof(WorkSessionDataResponse.Title)}
                  ,{nameof(WorkSessionDataResponse.Description)}
                  ,{nameof(WorkSessionDataResponse.LastModifierId)}
                  ,u.{nameof(UserDataResponse.FullName)} AS {nameof(WorkSessionDataResponse.LastModifierName)}
            FROM [TimeTrackerDb].[dbo].[WorkSession] ws
            INNER JOIN [TimeTrackerDb].[dbo].[User] u ON ws.{nameof(WorkSessionDataResponse.LastModifierId)} = u.{nameof(UserDataResponse.Id)}
            WHERE ws.{nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}
        ";

        using var connection = _context.GetConnection();
        var workSession = await connection.QuerySingleOrDefaultAsync<WorkSessionDataResponse>(query, new { id });

        return workSession;
    }

    public async Task<WorkSessionDataResponse> GetActiveWorkSessionByUserIdAsync(Guid userId)
    {
        var query = $@"
            SELECT ws.{nameof(WorkSessionDataResponse.Id)}
                  ,{nameof(WorkSessionDataResponse.UserId)}
                  ,{nameof(WorkSessionDataResponse.Start)}
                  ,[{nameof(WorkSessionDataResponse.End)}]
                  ,{nameof(WorkSessionDataResponse.Type)}
                  ,{nameof(WorkSessionDataResponse.Title)}
                  ,{nameof(WorkSessionDataResponse.Description)}
                  ,{nameof(WorkSessionDataResponse.LastModifierId)}
                  ,u.{nameof(UserDataResponse.FullName)} AS {nameof(WorkSessionDataResponse.LastModifierName)}
            FROM [TimeTrackerDb].[dbo].[WorkSession] ws
            INNER JOIN [TimeTrackerDb].[dbo].[User] u ON ws.{nameof(WorkSessionDataResponse.LastModifierId)} = u.{nameof(UserDataResponse.Id)}
            WHERE {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)} AND [{nameof(WorkSessionDataResponse.End)}] is NULL
        ";

        using var connection = _context.GetConnection();
        var workSession = await connection.QuerySingleOrDefaultAsync<WorkSessionDataResponse>(query, new { userId });

        return workSession;
    }

    public async Task<WorkSessionDataResponse> CreateWorkSessionAsync(WorkSessionDataRequest workSession)
    {
        var id = Guid.NewGuid();

        var query =
            $"INSERT INTO [WorkSession] (Id, {nameof(WorkSessionDataRequest.UserId)}, {nameof(WorkSessionDataRequest.LastModifierId)}, {nameof(WorkSessionDataRequest.Start)}, [{nameof(WorkSessionDataRequest.End)}], {nameof(WorkSessionDataRequest.Type)}, {nameof(WorkSessionDataRequest.Title)}, {nameof(WorkSessionDataRequest.Description)}) " +
            $"VALUES (@{nameof(id)}, " +
            $"@{nameof(WorkSessionDataRequest.UserId)}, " +
            $"@{nameof(WorkSessionDataRequest.LastModifierId)}, " +
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
            workSession.LastModifierId,
            workSession.Start,
            workSession.End,
            workSession.Type,
            workSession.Title,
            workSession.Description
        });

        var workSessionResponse = await GetWorkSessionByIdAsync(id);
        if (workSessionResponse is null)
        {
            throw new InvalidOperationException("Work session has not been added");
        }

        return workSessionResponse;
    }

    public async Task CreateWorkSessionsAsync(List<WorkSessionDataRequest> workSessionsList)
    {
        const string query =
            $"INSERT INTO [WorkSession] (Id, " +
            $"{nameof(WorkSessionDataResponse.UserId)}, " +
            $"{nameof(WorkSessionDataResponse.LastModifierId)}, " +
            $"{nameof(WorkSessionDataResponse.Start)}, " +
            $"[{nameof(WorkSessionDataResponse.End)}], " +
            $"{nameof(WorkSessionDataResponse.Type)}, " +
            $"{nameof(WorkSessionDataResponse.Title)}, " +
            $"{nameof(WorkSessionDataResponse.Description)}) " +
            $"VALUES (@{nameof(WorkSessionDataResponse.Id)}, " +
            $"@{nameof(WorkSessionDataResponse.UserId)}, " +
            $"@{nameof(WorkSessionDataResponse.LastModifierId)}, " +
            $"@{nameof(WorkSessionDataResponse.Start)}, " +
            $"@{nameof(WorkSessionDataResponse.End)}, " +
            $"@{nameof(WorkSessionDataResponse.Type)}, " +
            $"@{nameof(WorkSessionDataResponse.Title)}, " +
            $"@{nameof(WorkSessionDataResponse.Description)})";

        using var connection = _context.GetConnection();

        var listToInsert = new List<WorkSessionDataResponse>();
        foreach (var workSession in workSessionsList)
        {
            listToInsert.Add(new WorkSessionDataResponse()
            {
                Id = Guid.NewGuid(),
                UserId = workSession.UserId,
                LastModifierId = workSession.LastModifierId,
                Start = workSession.Start,
                End = workSession.End,
                Type = workSession.Type,
                Title = workSession.Title,
                Description = workSession.Description
            });
        }

        await connection.ExecuteAsync(query, listToInsert);
    }

    public async Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime)
    {
        const string query = $"UPDATE [WorkSession] SET [{nameof(WorkSessionDataResponse.End)}] = @{nameof(endDateTime)}," +
                             $" {nameof(WorkSessionDataResponse.Type)} = 'completed'" +
                             $" WHERE {nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { endDateTime, id });
    }

    public async Task UpdateWorkSessionAsync(Guid id, WorkSessionDataUpdateRequest workSession)
    {
        var query = "UPDATE [WorkSession] " +
                    $"SET [{nameof(WorkSessionDataResponse.Start)}] = @{nameof(workSession.Start)},  [{nameof(WorkSessionDataResponse.LastModifierId)}] = @{nameof(workSession.LastModifierId)}, " +
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
            workSession.LastModifierId,
            id
        });
    }

    public async Task DeleteWorkSessionAsync(Guid id)
    {
        const string query = $"DELETE FROM [WorkSession] WHERE [{nameof(WorkSessionDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { id });
    }
}