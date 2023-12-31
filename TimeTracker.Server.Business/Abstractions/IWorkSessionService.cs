﻿using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.WorkSession;

namespace TimeTracker.Server.Business.Abstractions;

public interface IWorkSessionService
{
    public Task<PaginationBusinessResponse<WorkSessionBusinessResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc, int? offset, 
        int? limit, DateTime? startDate, DateTime? endDate, bool? showPlanned = false);

    public Task<WorkSessionBusinessResponse> GetActiveWorkSessionByUserIdAsync(Guid userId);
  
    public Task<double> GetWorkingHoursByUserId(Guid userId, DateOnly start, DateOnly end);
  
    public Task<List<WorkSessionBusinessResponse>> GetWorkSessionsByUserIdsByMonthAsync(List<Guid> userIds, DateTime monthDate, bool hidePlanned = false);
  
    public Task<WorkSessionBusinessResponse> CreateWorkSessionAsync(WorkSessionBusinessRequest workSession);

    public Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime);

    public Task UpdateWorkSessionAsync(Guid id, WorkSessionBusinessUpdateRequest workSessionBusinessUpdateRequest);

    public Task DeleteWorkSessionAsync(Guid id);

}