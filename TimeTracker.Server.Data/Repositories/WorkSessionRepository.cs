using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared;

namespace TimeTracker.Server.Data.Repositories;

public class WorkSessionRepository : IWorkSessionRepository
{
    private readonly DapperContext _context;

    public WorkSessionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<PaginationDataResponse<WorkSessionDataResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc,
        int offset, int limit, DateTime? startDate, DateTime? endDate, bool? showPlanned = false)
    {
        var query = $"SELECT * FROM [WorkSession] WHERE";
        var countQuery = $"SELECT COUNT(*) FROM [WorkSession] WHERE";

        var withUserPart = $" {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)}";
        query += withUserPart;
        countQuery += withUserPart;

        if (startDate is not null)
        {
            string startDateQuery = $" AND {(endDate is not null && startDate is not null ? "(" : "")}DATEDIFF(DAY, " +
                $"[{nameof(WorkSessionDataResponse.Start)}], @{nameof(startDate)}) <= 0";

            query += startDateQuery;
            countQuery += startDateQuery;
        }

        if (endDate is not null)
        {
            var endDateQuery = $" AND DATEDIFF(DAY, [{nameof(WorkSessionDataResponse.Start)}], " +
                $"@{nameof(endDate)}) >= 0{(endDate is not null && startDate is not null ? ")" : "")}";

            query += endDateQuery;
            countQuery += endDateQuery;
        }

        if (showPlanned is not null && !(bool)showPlanned)
        {
            var withoutPlannedQuery = $" AND {nameof(WorkSessionDataResponse.Type)} != '{WorkSessionTypeEnum.Planned}'";
            query += withoutPlannedQuery;
            countQuery += withoutPlannedQuery;
        }

        var withoutActive = $" AND {nameof(WorkSessionDataResponse.Type)} != '{WorkSessionTypeEnum.Active}'";
        query += withoutActive;
        countQuery += withoutActive;

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

    public async Task<List<WorkSessionDataResponse>> GetUserWorkSessionsInRangeAsync(List<Guid> userIds, DateTime start, DateTime end)
    {
        var query = $"SELECT * FROM [WorkSession] WHERE" +
            $" (DATEDIFF(DAY, [{nameof(WorkSessionDataResponse.Start)}], @{nameof(start)}) <= 0" +
            $" AND DATEDIFF(DAY, [{nameof(WorkSessionDataResponse.Start)}], @{nameof(end)}) >= 0)";

        if (userIds.Any())
        {
            query += " AND (";
            var idsWhereRows = new List<string>();
            foreach (var id in userIds)
            {
                idsWhereRows.Add($"[{nameof(WorkSessionDataResponse.UserId)}] = '{id}'");
            }
            query += String.Join(" OR ", idsWhereRows.ToArray());
            query += ")";
        }
        else
        {
            return new List<WorkSessionDataResponse>();
        }

        using var connection = _context.GetConnection();
        var workSessionsDataResponse = await connection.QueryAsync<WorkSessionDataResponse>(query, new
        {
            start,
            end
        });

        return workSessionsDataResponse.ToList();
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
        var query = $"SELECT * FROM [WorkSession] WHERE" +
            $" {nameof(WorkSessionDataResponse.UserId)} = @{nameof(userId)}" +
            $" AND [{nameof(WorkSessionDataResponse.End)}] is NULL";

        using var connection = _context.GetConnection();
        var workSession = await connection.QuerySingleOrDefaultAsync<WorkSessionDataResponse>(query, new { userId });

        return workSession;
    }

    public async Task<WorkSessionDataResponse> CreateWorkSessionAsync(WorkSessionDataRequest workSession)
    {
        var id = Guid.NewGuid();

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

        await connection.ExecuteAsync(query, new
        {
            Id = id,
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
        var query = $"UPDATE [WorkSession] SET [{nameof(WorkSessionDataResponse.End)}] = @{nameof(endDateTime)}," +
                             $" {nameof(WorkSessionDataResponse.Type)} = '{WorkSessionTypeEnum.Completed}'" +
                             $" WHERE {nameof(WorkSessionDataResponse.Id)} = @{nameof(id)}";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { endDateTime, id });
    }

    public async Task UpdateWorkSessionAsync(Guid id, WorkSessionDataUpdateRequest workSession)
    {
        var query = $"UPDATE [WorkSession] SET" +
            $" [{nameof(WorkSessionDataUpdateRequest.Start)}] = @{nameof(workSession.Start)}," +
            $" [{nameof(WorkSessionDataUpdateRequest.End)}] = @{nameof(workSession.End)}," +
            $" [{nameof(WorkSessionDataUpdateRequest.LastModifierId)}] = @{nameof(workSession.LastModifierId)}," +
            $" [{nameof(WorkSessionDataUpdateRequest.Title)}] = @{nameof(workSession.Title)}," +
            $" [{nameof(WorkSessionDataUpdateRequest.Description)}] = @{nameof(workSession.Description)} " +
            $" WHERE [{nameof(WorkSessionDataResponse.Id)}] = @{nameof(id)}";

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

    public async Task DeleteWorkSessionsAsync(List<WorkSessionDataResponse> workSessionDataResponses)
    {
        const string query = $"DELETE FROM [WorkSession] WHERE [{nameof(WorkSessionDataResponse.Id)}] = @{nameof(WorkSessionDataResponse.Id)};";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, workSessionDataResponses);
    }

    public async Task DeleteWorkSessionsInRangeAsync(Guid userId, DateTime start, DateTime end, WorkSessionTypeEnum? type = null)
    {
        var query = $"DELETE FROM [WorkSession] WHERE" +
            $" [{nameof(WorkSessionDataResponse.UserId)}] = '{userId}'" +
            $" AND (DATEDIFF(DAY, [{nameof(WorkSessionDataResponse.Start)}], @{nameof(start)}) <= 0" +
            $" AND DATEDIFF(DAY, [{nameof(WorkSessionDataResponse.Start)}], @{nameof(end)}) >= 0)";

        if (type is not null)
        {
            query += $" AND [{nameof(WorkSessionDataResponse.Type)}] = '{type}'";
        }

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new
        {
            start,
            end
        });
    }
}