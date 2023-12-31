﻿using Dapper;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.Vacation;

namespace TimeTracker.Server.Data.Repositories;

public class VacationInfoRepository : IVacationInfoRepository
{
    private readonly DapperContext _context;

    public VacationInfoRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<VacationInfoDataResponse> GetVacationInfoByUserIdAsync(Guid userId)
    {
        const string query = $"SELECT * FROM [VacationInfo] WHERE [{nameof(VacationInfoDataResponse.UserId)}] = @{nameof(userId)};";

        using var connection = _context.GetConnection();
        var vacationInfoDataResponse = await connection.QuerySingleOrDefaultAsync<VacationInfoDataResponse>(query, new { userId });

        return vacationInfoDataResponse;
    }

    public async Task<VacationInfoDataResponse> CreateVacationInfoAsync(Guid userId)
    {
        var employmentDate = DateTime.UtcNow;
        const int daysSpent = 0;

        const string query =
            $"INSERT INTO [VacationInfo] (" +
            $"[{nameof(VacationInfoDataResponse.UserId)}], " +
            $"[{nameof(VacationInfoDataResponse.EmploymentDate)}], " +
            $"[{nameof(VacationInfoDataResponse.DaysSpent)}]" +
            $") VALUES (" +
            $"@{nameof(userId)}, " +
            $"@{nameof(employmentDate)}, " +
            $"@{nameof(daysSpent)}" +
            $");";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, new
        {
            userId,
            employmentDate,
            daysSpent
        });

        var vacationInfoDataResponse = await GetVacationInfoByUserIdAsync(userId);
        if (vacationInfoDataResponse is null)
        {
            throw new InvalidOperationException("Vacation info has not been added");
        }

        return vacationInfoDataResponse;
    }

    public async Task AddDaysSpentAsync(List<VacationInfoAddDaysSpendDataRequest> daysSpentData)
    {
        const string query = $"UPDATE [VacationInfo] SET " +
            $"[{nameof(VacationInfoAddDaysSpendDataRequest.DaysSpent)}] = " +
            $"[{nameof(VacationInfoAddDaysSpendDataRequest.DaysSpent)}] + @{nameof(VacationInfoAddDaysSpendDataRequest.DaysSpent)} " +
            $"WHERE [{nameof(VacationInfoAddDaysSpendDataRequest.UserId)}] = @{nameof(VacationInfoAddDaysSpendDataRequest.UserId)};";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, daysSpentData);
    }

    public async Task AddDaysSpentAsync(VacationInfoAddDaysSpendDataRequest daysSpentData)
    {
        const string query = $"UPDATE [VacationInfo] SET " +
            $"[{nameof(VacationInfoAddDaysSpendDataRequest.DaysSpent)}] = " +
            $"[{nameof(VacationInfoAddDaysSpendDataRequest.DaysSpent)}] + @{nameof(VacationInfoAddDaysSpendDataRequest.DaysSpent)} " +
            $"WHERE [{nameof(VacationInfoAddDaysSpendDataRequest.UserId)}] = @{nameof(VacationInfoAddDaysSpendDataRequest.UserId)};";

        using var connection = _context.GetConnection();

        await connection.ExecuteAsync(query, daysSpentData);
    }
}