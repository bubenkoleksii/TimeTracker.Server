using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Vacation;

namespace TimeTracker.Server.Data.Repositories;

public class VacationRepository : IVacationRepository
{
    private readonly DapperContext _context;

    public VacationRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<VacationDataResponse> GetVacationByIdAsync(Guid id)
    {
        const string query = $"SELECT * FROM [Vacation] WHERE [{nameof(VacationDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();
        var vacationDataResponse = await connection.QuerySingleOrDefaultAsync<VacationDataResponse>(query, new { id });

        return vacationDataResponse;
    }

    public async Task<IEnumerable<VacationDataResponse>> GetVacationsByUserIdAsync(Guid userId, bool? onlyApproved, bool orderByDesc)
    {
        var query = $"SELECT * FROM [Vacation]";

        var userWhereSection = $" WHERE [{nameof(VacationDataResponse.UserId)}] = @{nameof(userId)}";
        query += userWhereSection;
        if (onlyApproved is not null)
        {
            var approvedWhereSection = $" AND [{nameof(VacationDataResponse.IsApproved)}] = {((bool)onlyApproved ? "1" : "0")}";
            query += approvedWhereSection;
        }

        var orderByPostfix = orderByDesc ? "DESC" : "ASC";
        query += $" ORDER BY [{nameof(VacationDataResponse.Start)}] {orderByPostfix}, [{nameof(VacationDataResponse.End)}] {orderByPostfix}";

        using var connection = _context.GetConnection();
        var vacationsDataResponse = await connection.QueryAsync<VacationDataResponse>(query, new { userId });

        return vacationsDataResponse;
    }

    public async Task<IEnumerable<VacationDataResponse>> GetVacationRequestsAsync()
    {
        const string query = $"SELECT * FROM [Vacation] WHERE [{nameof(VacationDataResponse.IsApproved)}] IS NULL;";

        using var connection = _context.GetConnection();
        var vacationsDataResponse = await connection.QueryAsync<VacationDataResponse>(query);

        return vacationsDataResponse;
    }

    public async Task<IEnumerable<VacationDataResponse>> GetNotStartedUpdatedVacationsAsync()
    {
        const string query = $"SELECT * FROM [Vacation] WHERE " +
            $"[{nameof(VacationDataResponse.IsApproved)}] IS NOT NULL" +
            $" AND [{nameof(VacationDataResponse.Start)}] > GETDATE()" +
            $";";

        using var connection = _context.GetConnection();
        var vacationsDataResponse = await connection.QueryAsync<VacationDataResponse>(query);

        return vacationsDataResponse;
    }

    public async Task<IEnumerable<VacationDataResponse>> GetApprovedNotFinishedVacationsAsync()
    {
        const string query = $"SELECT * FROM [Vacation] WHERE " +
            $"[{nameof(VacationDataResponse.IsApproved)}] = 1" +
            $" AND DATEDIFF(DAY, [{nameof(VacationDataResponse.End)}], GETDATE()) <= 0" +
            $";";

        using var connection = _context.GetConnection();
        var vacationsDataResponse = await connection.QueryAsync<VacationDataResponse>(query);

        return vacationsDataResponse;
    }

    public async Task<VacationDataResponse> CreateVacationAsync(VacationDataRequest vacationDataRequest)
    {
        var id = Guid.NewGuid();

        const string query =
            $"INSERT INTO [Vacation] (" +
            $"[{nameof(VacationDataResponse.Id)}], " +
            $"[{nameof(VacationDataResponse.UserId)}], " +
            $"[{nameof(VacationDataResponse.Start)}], " +
            $"[{nameof(VacationDataResponse.End)}], " +
            $"[{nameof(VacationDataResponse.Comment)}], " +
            $"[{nameof(VacationDataResponse.IsApproved)}], " +
            $"[{nameof(VacationDataResponse.ApproverId)}], " +
            $"[{nameof(VacationDataResponse.ApproverComment)}]" +
            $") VALUES (" +
            $"@{nameof(id)}, " +
            $"@{nameof(vacationDataRequest.UserId)}, " +
            $"@{nameof(vacationDataRequest.Start)}, " +
            $"@{nameof(vacationDataRequest.End)}, " +
            $"@{nameof(vacationDataRequest.Comment)}, " +
            $"NULL, " +
            $"NULL, " +
            $"NULL" +
            $");";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            id,
            vacationDataRequest.UserId,
            vacationDataRequest.Start,
            vacationDataRequest.End,
            vacationDataRequest.Comment
        });

        var vacationDataResponse = await GetVacationByIdAsync(id);
        if (vacationDataResponse is null)
        {
            throw new InvalidOperationException("Vacation request has not been added");
        }

        return vacationDataResponse;
    }

    public async Task ApproverUpdateVacationAsync(VacationApproveDataRequest vacationApproveDataRequest)
    {
        const string query = $"UPDATE [Vacation] SET " +
            $"[{nameof(VacationDataResponse.IsApproved)}] = @{nameof(vacationApproveDataRequest.IsApproved)}, " +
            $"[{nameof(VacationDataResponse.ApproverId)}] = @{nameof(vacationApproveDataRequest.ApproverId)}, " +
            $"[{nameof(VacationDataResponse.ApproverComment)}] = @{nameof(vacationApproveDataRequest.ApproverComment)} " +
            $"WHERE [{nameof(VacationDataResponse.Id)}] = @{nameof(vacationApproveDataRequest.Id)};";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            vacationApproveDataRequest.IsApproved,
            vacationApproveDataRequest.ApproverId,
            vacationApproveDataRequest.ApproverComment,
            vacationApproveDataRequest.Id
        });
    }

    public async Task DeleteVacationAsync(Guid id)
    {
        const string query = $"DELETE FROM [Vacation] WHERE [{nameof(VacationDataResponse.Id)}] = @{nameof(id)};";

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(query, new { id });
    }
}