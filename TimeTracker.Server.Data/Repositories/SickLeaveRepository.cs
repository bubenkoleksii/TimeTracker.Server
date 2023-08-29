using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.SickLeave;

namespace TimeTracker.Server.Data.Repositories;

public class SickLeaveRepository : ISickLeaveRepository
{
    private readonly DapperContext _context;

    public SickLeaveRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<SickLeaveDataResponse> GetSickLeaveByIdAsync(Guid id)
    {
        const string query = $"SELECT * FROM [SickLeave] WHERE [{nameof(SickLeaveDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();
        var sickLeaveDataResponse = await connection.QuerySingleOrDefaultAsync<SickLeaveDataResponse>(query, new { id });

        return sickLeaveDataResponse;
    }

    public async Task<List<SickLeaveDataResponse>> GetSickLeavesAsync(DateTime? date = null, Guid? userId = null, bool searchByYear = false)
    {
        var query = $"SELECT * FROM [SickLeave] WHERE 1 = 1";

        if (date is not null)
        {
            query += $" AND DATEDIFF({(searchByYear ? "YEAR" : "MONTH")}, [{nameof(SickLeaveDataResponse.Start)}], @{nameof(date)}) = 0";
        }

        if (userId is not null)
        {
            query += $" AND [{nameof(SickLeaveDataResponse.UserId)}] = '{userId}'";
        }

        query += $" ORDER BY [{nameof(SickLeaveDataResponse.Start)}] DESC";

        using var connection = _context.GetConnection();
        var sickLeavesDataResponse = await connection.QueryAsync<SickLeaveDataResponse>(query, new { date });

        return sickLeavesDataResponse.ToList();
    }

    public async Task<List<SickLeaveDataResponse>> GetUsersSickLeaveInRangeAsync(List<Guid> userIds, DateTime start, DateTime end)
    {
        var query = $"SELECT * FROM [SickLeave] WHERE" +
            $" (((DATEDIFF(DAY, [{nameof(SickLeaveDataResponse.Start)}], @{nameof(start)}) <= 0" +
            $" AND DATEDIFF(DAY, [{nameof(SickLeaveDataResponse.Start)}], @{nameof(end)}) >= 0)" +
            $" OR (DATEDIFF(DAY, [{nameof(SickLeaveDataResponse.End)}], @{nameof(start)}) <= 0" +
            $" AND DATEDIFF(DAY, [{nameof(SickLeaveDataResponse.End)}], @{nameof(end)}) >= 0)))";

        if (userIds.Any())
        {
            query += " AND (";
            var idsWhereRows = new List<string>();
            foreach (var id in userIds)
            {
                idsWhereRows.Add($"[{nameof(SickLeaveDataResponse.UserId)}] = '{id}'");
            }
            query += String.Join(" OR ", idsWhereRows.ToArray());
            query += ")";
        }
        else
        {
            return new List<SickLeaveDataResponse>();
        }

        using var connection = _context.GetConnection();
        var workSessionsDataResponse = await connection.QueryAsync<SickLeaveDataResponse>(query, new
        {
            start,
            end
        });

        return workSessionsDataResponse.ToList();
    }

    public async Task<SickLeaveDataResponse> CreateSickLeaveAsync(SickLeaveDataRequest sickLeaveDataRequest)
    {
        var id = Guid.NewGuid();

        const string query = $"INSERT INTO [SickLeave] (" +
            $"[{nameof(SickLeaveDataResponse.Id)}], " +
            $"[{nameof(SickLeaveDataResponse.UserId)}], " +
            $"[{nameof(SickLeaveDataResponse.LastModifierId)}], " +
            $"[{nameof(SickLeaveDataResponse.Start)}], " +
            $"[{nameof(SickLeaveDataResponse.End)}]" +
            $") VALUES (" +
            $"@{nameof(SickLeaveDataResponse.Id)}, " +
            $"@{nameof(SickLeaveDataResponse.UserId)}, " +
            $"@{nameof(SickLeaveDataResponse.LastModifierId)}, " +
            $"@{nameof(SickLeaveDataResponse.Start)}, " +
            $"@{nameof(SickLeaveDataResponse.End)});";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            Id = id,
            sickLeaveDataRequest.UserId,
            sickLeaveDataRequest.LastModifierId,
            sickLeaveDataRequest.Start,
            sickLeaveDataRequest.End
        });

        var sickLeaveDataResponse = await GetSickLeaveByIdAsync(id);
        if (sickLeaveDataResponse is null)
        {
            throw new InvalidOperationException("Sick leave data has not been added");
        }

        return sickLeaveDataResponse;
    }

    public async Task UpdateSickLeaveAsync(Guid id, SickLeaveDataRequest sickLeaveDataRequest)
    {
        const string query = $"UPDATE [SickLeave] SET " +
            $"[{nameof(SickLeaveDataResponse.UserId)}] = @{nameof(SickLeaveDataResponse.UserId)}, " +
            $"[{nameof(SickLeaveDataResponse.LastModifierId)}] = @{nameof(SickLeaveDataResponse.LastModifierId)}, " +
            $"[{nameof(SickLeaveDataResponse.Start)}] = @{nameof(SickLeaveDataResponse.Start)}, " +
            $"[{nameof(SickLeaveDataResponse.End)}] = @{nameof(SickLeaveDataResponse.End)} " +
            $"WHERE [{nameof(SickLeaveDataResponse.Id)}] = @{nameof(SickLeaveDataResponse.Id)};";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            Id = id,
            sickLeaveDataRequest.UserId,
            sickLeaveDataRequest.LastModifierId,
            sickLeaveDataRequest.Start,
            sickLeaveDataRequest.End
        });
    }

    public async Task DeleteSickLeaveAsync(Guid id)
    {
        const string query = $"DELETE FROM [SickLeave] WHERE [{nameof(SickLeaveDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { id });
    }
}