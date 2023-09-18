using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Holidays;

namespace TimeTracker.Server.Data.Repositories;

public class HolidayRepository : IHolidayRepository
{
    private readonly DapperContext _context;

    public HolidayRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<HolidayDataResponse> GetHolidayByIdAsync(Guid id)
    {
        const string query = $"SELECT * FROM [Holidays] WHERE [{nameof(HolidayDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();
        var holiday = await connection.QuerySingleOrDefaultAsync<HolidayDataResponse>(query, new { id });

        return holiday;
    }

    public async Task<IEnumerable<HolidayDataResponse>> GetHolidaysAsync()
    {
        const string query = $"SELECT * FROM [Holidays] ORDER BY [{nameof(HolidayDataResponse.Date)}];";

        using var connection = _context.GetConnection();
        var holidays = await connection.QueryAsync<HolidayDataResponse>(query);

        return holidays;
    }

    public async Task<List<HolidayDataResponse>> GetHolidaysByDateAsync(DateOnly date, HolidayTypesEnum? holidayType = null)
    {
        var query = $"SELECT * FROM [Holidays] WHERE" +
            $" ([{nameof(HolidayDataResponse.EndDate)}] IS NULL AND DATEDIFF(DAY, {nameof(HolidayDataResponse.Date)}, @{nameof(date)}) = 0)" +
            $" OR (DATEDIFF(DAY, [{nameof(HolidayDataResponse.Date)}], @{nameof(date)}) >= 0" +
            $" AND DATEDIFF(DAY, [{nameof(HolidayDataResponse.EndDate)}], @{nameof(date)}) <= 0)";

        if (holidayType is not null)
        {
            query += $" AND [{nameof(HolidayDataResponse.Type)}] = @{nameof(holidayType)}";
        }

        using var connection = _context.GetConnection();
        var holidayDataResponse = await connection.QueryAsync<HolidayDataResponse>(query, new
        {
            date = date.ToDateTime(new TimeOnly()),
            holidayType = holidayType.ToString()
        });

        return holidayDataResponse.ToList();
    }

    public async Task<List<HolidayDataResponse>> GetHolidaysByDateRangeAsync(DateOnly start, DateOnly end)
    {
        const string query = $"SELECT * FROM [Holidays] WHERE" +
            $" (((DATEDIFF(DAY, [{nameof(HolidayDataResponse.Date)}], @{nameof(start)}) <= 0" +
            $" AND DATEDIFF(DAY, [{nameof(HolidayDataResponse.Date)}], @{nameof(end)}) >= 0)" +
            $" OR (DATEDIFF(DAY, [{nameof(HolidayDataResponse.EndDate)}], @{nameof(start)}) <= 0" +
            $" AND DATEDIFF(DAY, [{nameof(HolidayDataResponse.EndDate)}], @{nameof(end)}) >= 0)))";

        using var connection = _context.GetConnection();
        var holidayDataResponse = await connection.QueryAsync<HolidayDataResponse>(query, new
        {
            start = start.ToDateTime(new TimeOnly()),
            end = end.ToDateTime(new TimeOnly())
        });

        return holidayDataResponse.ToList();
    }

    public async Task<HolidayDataResponse> CreateHolidayAsync(HolidayDataRequest holidayDataRequest)
    {
        var id = Guid.NewGuid();

        var query =
            $"INSERT INTO [Holidays] ([Id], [{nameof(HolidayDataResponse.Title)}], [{nameof(HolidayDataResponse.Type)}], [{nameof(HolidayDataResponse.Date)}], " +
            $"[{nameof(HolidayDataResponse.EndDate)}]) VALUES (@{nameof(id)}, @{nameof(holidayDataRequest.Title)}, @{nameof(holidayDataRequest.Type)}, " +
            $"@{nameof(holidayDataRequest.Date)}, {(holidayDataRequest.EndDate is not null ? $"@{nameof(holidayDataRequest.EndDate)}" : "NULL")});";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            id,
            holidayDataRequest.Title,
            holidayDataRequest.Type,
            holidayDataRequest.Date,
            holidayDataRequest.EndDate
        });

        var hoilidayResponse = await GetHolidayByIdAsync(id);
        if (hoilidayResponse is null)
        {
            throw new InvalidOperationException("Holiday has not been added");
        }

        return hoilidayResponse;
    }

    public async Task UpdateHolidayAsync(Guid id, HolidayDataRequest holidayDataRequest)
    {
        var query = "UPDATE [Holidays] " +
                    $"SET [{nameof(HolidayDataResponse.Title)}] = @{nameof(holidayDataRequest.Title)}, " +
                    $"[{nameof(HolidayDataResponse.Type)}] = @{nameof(holidayDataRequest.Type)}, " +
                    $"[{nameof(HolidayDataResponse.Date)}] = @{nameof(holidayDataRequest.Date)}, " +
                    $"[{nameof(HolidayDataResponse.EndDate)}] = {(holidayDataRequest.EndDate is not null ? $"@{nameof(holidayDataRequest.EndDate)}" : "NULL")} " +
                    $"WHERE [{nameof(HolidayDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            holidayDataRequest.Title,
            holidayDataRequest.Type,
            holidayDataRequest.Date,
            holidayDataRequest.EndDate,
            id
        });
    }

    public async Task DeleteHolidayAsync(Guid id)
    {
        const string query = $"DELETE FROM [Holidays] WHERE [{nameof(HolidayDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { id });
    }
}