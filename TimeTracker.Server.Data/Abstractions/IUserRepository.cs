using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Abstractions;

public interface IUserRepository
{
    public Task<UserDataResponse> GetUserByIdAsync(Guid id);

    public Task<UserDataResponse> GetUserByEmailAsync(string email);

    public Task<List<UserDataResponse>> GetFullTimeWorkingUsersAsync();

    public Task<PaginationDataResponse<UserDataResponse>> GetAllUsersAsync(int offset, int limit, string search, int? filteringEmploymentRate, string? filteringStatus, string? sortingColumn);
    
    public Task<IEnumerable<UserDataResponse>> GetAllUsersAsync();

    public Task<UserDataResponse> CreateUserAsync(UserDataRequest userRequest);

    public Task<UserDataResponse> UpdateUserAsync(UserDataRequest userRequest, Guid id);

    public Task DeactivateUserAsync(Guid id);

    public Task SetRefreshTokenAsync(string refreshToken, Guid id);

    public Task RemoveRefreshAsync(Guid id);

    public Task AddSetPasswordLinkAsync(Guid setPasswordLink, DateTime expired, Guid id);

    public Task SetPasswordAsync(SetPasswordUserDataRequest user);

    public Task SetUserStatusAsync(Guid id, string status);

    public Task SetUserStatusAsync(List<UserSetStatusDataRequest> userSetStatusDataRequests);

    public Task RemovePasswordAsync(Guid id);
}