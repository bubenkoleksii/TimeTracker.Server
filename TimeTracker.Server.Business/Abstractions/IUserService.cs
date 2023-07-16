﻿using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.User;

namespace TimeTracker.Server.Business.Abstractions;

public interface IUserService
{
    public Task<UserBusinessResponse> CreateUserAsync(UserBusinessRequest userRequest);

    public Task<PaginationBusinessResponse<UserBusinessResponse>> GetAllUsersAsync(int? offset, int? limit, string search, int? filteringEmploymentRate, string? sortingColumn);

    public Task FireUserAsync(Guid id);

    public Task AddSetPasswordLinkAsync(string email);

    public Task SetPasswordAsync(SetPasswordUserBusinessRequest userRequest);

    public Task ResetPasswordAsync();
}