using System.Security.Claims;
using AutoMapper;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Shared;
using TimeTracker.Server.Shared.Exceptions;
using ExecutionError = GraphQL.ExecutionError;

namespace TimeTracker.Server.Business.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;

    private readonly IMailService _mailService;

    private readonly IUserRepository _userRepository;

    private readonly IVacationInfoRepository _vacationInfoRepository;

    private readonly IConfiguration _configuration;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IHolidayService _holidayService;

    private readonly IWorkSessionRepository _workSessionRepository;

    private readonly IVacationRepository _vacationRepository;

    private readonly ISickLeaveRepository _sickLeaveRepository;

    public UserService(IMailService mailService, IUserRepository userRepository, IVacationInfoRepository vacationInfoRepository, IMapper mapper, 
        IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHolidayService holidayService, IWorkSessionRepository workSessionRepository,
        IVacationRepository vacationRepository, ISickLeaveRepository sickLeaveRepository)
    {
        _mailService = mailService;
        _userRepository = userRepository;
        _mapper = mapper;
        _vacationInfoRepository = vacationInfoRepository;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _holidayService = holidayService;
        _workSessionRepository = workSessionRepository;
        _vacationRepository = vacationRepository;
        _sickLeaveRepository = sickLeaveRepository;
    }

    public async Task<UserBusinessResponse> UpdateUserAsync(UserBusinessRequest userRequest, Guid id)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            throw new ExecutionError($"User with id {id} not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        if (existingUser.Status == UserStatusEnum.deactivated.ToString())
        {
            throw new ExecutionError($"User with id {id} cannot be updated because they are deactivated")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_ALREADY_EXISTS.ToString()
            };
        }

        var userDataRequest = _mapper.Map<UserDataRequest>(userRequest);

        var userDataResponse = await _userRepository.UpdateUserAsync(userDataRequest, id);
        if (userDataResponse.SetPasswordLink != null && userDataResponse.SetPasswordLinkExpired > DateTime.UtcNow)
        {
            userDataResponse.HasValidSetPasswordLink = true;
        }

        var userBusinessResponse = _mapper.Map<UserBusinessResponse>(userDataResponse);
        return userBusinessResponse;
    }

    public async Task<PaginationBusinessResponse<UserBusinessResponse>> GetPaginatedUsersAsync(int? offset, int? limit, string search, int? filteringEmploymentRate, string? filteringStatus, string? sortingColumn)
    {
        var limitDefault = int.Parse(_configuration.GetSection("Pagination:UserLimit").Value);

        var validatedOffset = offset is >= 0 ? offset.Value : default;
        var validatedLimit = limit is > 0 ? limit.Value : limitDefault;

        var usersDataResponse = await _userRepository.GetAllUsersAsync(validatedOffset, validatedLimit, search, filteringEmploymentRate, filteringStatus, sortingColumn);

        var usersBusinessResponse = _mapper.Map<PaginationBusinessResponse<UserBusinessResponse>>(usersDataResponse);
        return usersBusinessResponse;
    }

    public async Task<PaginationBusinessResponse<UserWorkInfoBusinessResponse>> GetAllUsersWorkInfoAsync(int? offset, int? limit, string search, int? filteringEmploymentRate,
        string? filteringStatus, string? sortingColumn, DateTime? start, DateTime? end, bool? withoutPagination = false)
    {
        var limitDefault = int.Parse(_configuration.GetSection("Pagination:UserLimit").Value);

        var validatedOffset = 0;
        var validatedLimit = int.MaxValue;

        if (withoutPagination is null or false)
        {
            validatedOffset = offset is >= 0 ? offset.Value : default;
            validatedLimit = limit is > 0 ? limit.Value : limitDefault;
        }

        var usersDataResponse = await _userRepository.GetAllUsersAsync(validatedOffset, validatedLimit, search, filteringEmploymentRate, filteringStatus, sortingColumn);
        if (usersDataResponse.Items == null)
        {
            return new PaginationBusinessResponse<UserWorkInfoBusinessResponse>()
            {
                Count = 0,
                Items = null
            };
        }

        var launchDate = DateOnly.Parse(_configuration.GetSection("LaunchDate").Value);
        var validatedStart = start == null ? launchDate : DateOnly.FromDateTime((DateTime)start);
        var validatedEnd = end == null ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.FromDateTime((DateTime)end);
        
        var countOfWorkingDays = await _holidayService.GetCountOfWorkingDays(validatedStart, validatedEnd);

        var fullDayWorkingHours = int.Parse(_configuration.GetSection("WorkHours:FullDay").Value);
        var shortDayWorkingHours = int.Parse(_configuration.GetSection("WorkHours:ShortDay").Value);

        var fullTimeSummaryHours = countOfWorkingDays.FullDays * fullDayWorkingHours + countOfWorkingDays.ShortDays * shortDayWorkingHours;

        var usersWorkInfoList = new List<UserWorkInfoBusinessResponse>();
        foreach (var user in usersDataResponse.Items)
        {
            var userWorkInfoResponse = await GetUserWorkInfoAsync(user, fullTimeSummaryHours, fullDayWorkingHours, shortDayWorkingHours, validatedStart.ToDateTime(new TimeOnly(0, 0)), validatedEnd.ToDateTime(new TimeOnly(0, 0)));
            usersWorkInfoList.Add(userWorkInfoResponse);
        }

        var usersWorkInfoBusinessResponse = new PaginationBusinessResponse<UserWorkInfoBusinessResponse>()
        {
            Count = usersDataResponse.Count,
            Items = usersWorkInfoList
        };

        return usersWorkInfoBusinessResponse;
    }

    private async Task<UserWorkInfoBusinessResponse> GetUserWorkInfoAsync(UserDataResponse user, int fullTimeSummaryHours, int fullDayWorkingHours, int shortDayWorkingHours, DateTime validatedStart, DateTime validatedEnd)
    {
        var userWorkInfoResponse = _mapper.Map<UserWorkInfoBusinessResponse>(user);

        var countOfWorkingHours = await GetCountOfWorkingHours(user, validatedStart, validatedEnd);

        userWorkInfoResponse.PlannedWorkingHours = Math.Round(fullTimeSummaryHours * user.EmploymentRate / 100.0, 2);
        userWorkInfoResponse.WorkedHours = countOfWorkingHours;

        var summaryVacationHours = await GetCountOfVacationHours(user.Id, fullDayWorkingHours, shortDayWorkingHours, validatedStart, validatedEnd);

        var summarySickLeaveHours = await GetCountOfSickLeaveHours(user.Id, fullDayWorkingHours, shortDayWorkingHours, validatedStart, validatedEnd);

        userWorkInfoResponse.SickLeaveHours = Math.Round(summarySickLeaveHours * user.EmploymentRate / 100.0, 2);
        userWorkInfoResponse.VacationHours = Math.Round(summaryVacationHours * user.EmploymentRate / 100.0, 2);

        return userWorkInfoResponse;
    }

    private async Task<double> GetCountOfWorkingHours(UserDataResponse user, DateTime validatedStart, DateTime validatedEnd)
    {
        var workSessionsDataResponse = await _workSessionRepository.GetOneUserWorkSessionsInRangeAsync(user.Id, validatedStart, validatedEnd, type: WorkSessionTypeEnum.Completed);
        if (user.EmploymentRate == 100)
        {
            var autoWorkSessionsDataResponse = await _workSessionRepository.GetOneUserWorkSessionsInRangeAsync(user.Id, validatedStart, validatedEnd, type: WorkSessionTypeEnum.Auto);
            workSessionsDataResponse.AddRange(autoWorkSessionsDataResponse);
        }

        var countOfWorkingHours = Math.Round(workSessionsDataResponse.Sum(workSession => (workSession.End - workSession.Start).Value.TotalHours), 2);
        return countOfWorkingHours;
    }

    private async Task<double> GetCountOfVacationHours(Guid userId, int fullDayWorkingHours, int shortDayWorkingHours, DateTime validatedStart, DateTime validatedEnd)
    {
        var vacationsDataResponse = await _vacationRepository.GetUsersVacationsInRangeAsync(new List<Guid> { userId }, validatedStart, validatedEnd);

        var summaryVacationHours = 0;
        foreach (var vacation in vacationsDataResponse)
        {
            var vacationStart = validatedStart > vacation.Start ? validatedStart : vacation.Start;
            var vacationEnd = validatedEnd > vacation.End ? vacation.End : validatedEnd;

            var vacationDays = await _holidayService.GetCountOfWorkingDays(DateOnly.FromDateTime(vacationStart), DateOnly.FromDateTime(vacationEnd));
            summaryVacationHours += vacationDays.FullDays * fullDayWorkingHours + vacationDays.ShortDays * shortDayWorkingHours;
        }

        return summaryVacationHours;
    }

    private async Task<double> GetCountOfSickLeaveHours(Guid userId, int fullDayWorkingHours, int shortDayWorkingHours, DateTime validatedStart, DateTime validatedEnd)
    {
        var sickLeaveDataResponse = await _sickLeaveRepository.GetUsersSickLeaveInRangeAsync(new List<Guid> { userId }, validatedStart, validatedEnd);

        var summarySickLeaveHours = 0;
        foreach (var sickLeave in sickLeaveDataResponse)
        {
            var sickLeaveStart = validatedStart > sickLeave.Start ? validatedStart : sickLeave.Start;
            var sickLeaveEnd = validatedEnd > sickLeave.End ? sickLeave.End : validatedEnd;

            var sickLeaveDays = await _holidayService.GetCountOfWorkingDays(DateOnly.FromDateTime(sickLeaveStart), DateOnly.FromDateTime(sickLeaveEnd));
            summarySickLeaveHours += sickLeaveDays.FullDays * fullDayWorkingHours + sickLeaveDays.ShortDays * shortDayWorkingHours;
        }

        return summarySickLeaveHours;
    }

    public async Task<List<UserBusinessResponse>> GetUsersByIds(List<Guid> ids)
    {
        var userDataResponseList = await _userRepository.GetUserByIdAsync(ids);
        var userBusinessResponseList = _mapper.Map<List<UserBusinessResponse>>(userDataResponseList);
        return userBusinessResponseList;
    }

    public async Task<byte[]> ExportUsersWorkInfoToExcel(string search, int? filteringEmploymentRate, string? filteringStatus, string? sortingColumn, DateTime? start, DateTime? end)
    {
        var usersDataResponse = await GetAllUsersWorkInfoAsync(0, int.MaxValue, search, filteringEmploymentRate, filteringStatus, sortingColumn, start, end, true);
        if (usersDataResponse.Items == null)
        {
            throw new ExecutionError("Users not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        var formattedStartDate = start.HasValue ? start.Value.ToString("M/d/yyyy") : "";
        var formattedEndDate = end.HasValue ? end.Value.ToString("M/d/yyyy") : "";

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add($"Sheet1");

        worksheet.Cells["A1:F1"].Merge = true;
        worksheet.Cells["A1:F1"].Style.Font.Bold = true;
        worksheet.Cells["A1:F1"].Style.Font.Size = 16;
        worksheet.Cells["A1:F1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        worksheet.Cells["A1:F1"].Value = $"Users Work Information ({formattedStartDate} - {formattedEndDate})";

        worksheet.Row(2).Height = 20;

        worksheet.Cells["A3"].Value = "Full Name";
        worksheet.Cells["A3"].Style.Font.Bold = true;
        worksheet.Cells["B3"].Value = "Email";
        worksheet.Cells["B3"].Style.Font.Bold = true;
        worksheet.Cells["C3"].Value = "Employment Rate";
        worksheet.Cells["C3"].Style.Font.Bold = true;
        worksheet.Cells["D3"].Value = "Worked Hours";
        worksheet.Cells["D3"].Style.Font.Bold = true;
        worksheet.Cells["E3"].Value = "Planned Hours";
        worksheet.Cells["E3"].Style.Font.Bold = true;
        worksheet.Cells["F3"].Value = "Work Percentage";
        worksheet.Cells["F3"].Style.Font.Bold = true;
        worksheet.Cells["G3"].Value = "Vacation Hours";
        worksheet.Cells["G3"].Style.Font.Bold = true;
        worksheet.Cells["H3"].Value = "Sick Leave Hours";
        worksheet.Cells["H3"].Style.Font.Bold = true;

        worksheet.Column(1).Width = 40;
        worksheet.Column(2).Width = 40;
        worksheet.Column(3).Width = 20;
        worksheet.Column(4).Width = 20;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 20;
        worksheet.Column(8).Width = 20;

        var startRow = 4;
        foreach (var info in usersDataResponse.Items)
        {
            worksheet.Cells[startRow, 1].Value = info.FullName;
            worksheet.Cells[startRow, 2].Value = info.Email;
            worksheet.Cells[startRow, 3].Value = info.EmploymentRate;
            worksheet.Cells[startRow, 4].Value = info.WorkedHours;
            worksheet.Cells[startRow, 5].Value = info.PlannedWorkingHours;
            worksheet.Cells[startRow, 6].FormulaR1C1 = "ROUND(RC[-2]/RC[-1]*100, 2)";
            worksheet.Cells[startRow, 7].Value = info.VacationHours;
            worksheet.Cells[startRow, 8].Value = info.SickLeaveHours;

            startRow++;
        }

        var excelBytes = await package.GetAsByteArrayAsync();
        return excelBytes;
    }

    public async Task<IEnumerable<UserBusinessResponse>> GetAllUsersAsync(bool showFired = false)
    {
        var userDataResponseList = await _userRepository.GetAllUsersAsync(showFired);
        var userBusinessResponseList = _mapper.Map<IEnumerable<UserBusinessResponse>>(userDataResponseList);
        return userBusinessResponseList;
    }

    public async Task DeactivateUserAsync(Guid id)
    {
        var candidate = await _userRepository.GetUserByIdAsync(id);
        if (candidate == null)
        {
            throw new ExecutionError($"User with id {id} not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        if (candidate.Status == UserStatusEnum.deactivated.ToString())
            return;

        await _userRepository.DeactivateUserAsync(id);

        var user = await _userRepository.GetUserByIdAsync(id);
        if (user.Status != UserStatusEnum.deactivated.ToString())
        {
            throw new ExecutionError($"User with id {id} not deactivated")
            {
                Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
            };
        }
    }

    public async Task<UserBusinessResponse> CreateUserAsync(UserBusinessRequest userRequest)
    {
        var candidate = await _userRepository.GetUserByEmailAsync(userRequest.Email);
        if (candidate != null)
        {
            throw new ExecutionError($"User with email {userRequest.Email} already exists")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_ALREADY_EXISTS.ToString()
            };
        }

        var userDataRequest = _mapper.Map<UserDataRequest>(userRequest);

        var userDataResponse = await _userRepository.CreateUserAsync(userDataRequest);

        var userBusinessResponse = _mapper.Map<UserBusinessResponse>(userDataResponse);
        return userBusinessResponse;
    }

    public async Task AddSetPasswordLinkAsync(string email)
    {
        var candidate = await _userRepository.GetUserByEmailAsync(email);
        if (candidate == null)
        {
            throw new ExecutionError($"User with email {email} not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        if (candidate.HasPassword)
        {
            throw new ExecutionError($"User with email {email} already set a password")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_HAS_PASSWORD.ToString()
            };
        }

        if (candidate.SetPasswordLink != null && candidate.SetPasswordLinkExpired > DateTime.UtcNow)
        {
            throw new ExecutionError($"For user with email {email} already set a password link")
            {
                Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
            };
        }

        var hoursExpired = int.Parse(_configuration.GetSection("Password:SetHoursExpired").Value);
        var expired = DateTime.Now.AddHours(hoursExpired);

        var setPasswordLink = Guid.NewGuid();
        var setPasswordUrl = $"{_configuration.GetSection("Client:Url").Value}set-password/{setPasswordLink}/";

        var subject = "TimeTracker: Please set a password for your account";
        var text = @$"
            <div>
                <h1>Set Password for Your Account</h1>
                <p>In order to log in to your account, you need to set a password for it before {expired:dd.MM.yyyy HH:mm}.</p>
                <p>Click on the button below and follow the link to set a password</p>
                <a href=""{setPasswordUrl}"" style=""display:inline-block; background-color:#4CAF50; color:white; padding:10px 20px; text-decoration:none;"">Set Password</a>
            </div>";
        try
        {
            await _mailService.SendTextMessageAsync(email, subject, text);
        }
        catch
        {
            throw new ExecutionError($"Could not send an email {email} to set a password")
            {
                Code = GraphQLCustomErrorCodesEnum.SEND_EMAIL_FAILED.ToString()
            };
        }

        await _userRepository.AddSetPasswordLinkAsync(setPasswordLink, expired.ToUniversalTime(), candidate.Id);
    }

    public async Task SetPasswordAsync(SetPasswordUserBusinessRequest userRequest)
    {
        var candidate = await _userRepository.GetUserByEmailAsync(userRequest.Email);
        if (candidate == null)
        {
            throw new ExecutionError($"User with email {userRequest.Email} not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        if (candidate.HasPassword)
        {
            throw new ExecutionError($"User with email {userRequest.Email} has already set a password")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_HAS_PASSWORD.ToString()
            };
        }

        if (candidate.SetPasswordLinkExpired < DateTime.UtcNow)
        {
            throw new ExecutionError($"Password link expired for user with email {userRequest.Email}")
            {
                Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
            };
        }

        if (candidate.SetPasswordLink != new Guid(userRequest.SetPasswordLink))
        {
            throw new ExecutionError($"User with email {userRequest.Email} used the wrong link")
            {
                Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
            };
        }

        var userDataRequest = _mapper.Map<SetPasswordUserDataRequest>(userRequest);
        userDataRequest.HashPassword = HashPassword(userRequest.Password);

        await _userRepository.SetPasswordAsync(userDataRequest);

        await _vacationInfoRepository.CreateVacationInfoAsync(candidate.Id);
    }

    public async Task ResetPasswordAsync()
    {
        var claims = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).Claims;

        var userId = claims.FirstOrDefault(c => c.Type == "Id");
        if (userId == null)
        {
            var error = new ExecutionError("Claim user id not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
            throw error;
        }

        var user = await _userRepository.GetUserByIdAsync(Guid.Parse(userId.Value));

        var hoursExpired = int.Parse(_configuration.GetSection("Password:ResetHoursExpired").Value);
        var expired = DateTime.Now.AddHours(hoursExpired);

        var setPasswordLink = Guid.NewGuid();
        var setPasswordUrl = $"{_configuration.GetSection("Client:Url").Value}set-password/{setPasswordLink}/";

        var subject = "TimeTracker: Please reset a password for your account";
        var text = @$"
            <div>
                <h1>Reset Password for Your Account</h1>
                <p>In order to log in to your account, you need to reset a password for it before {expired:dd.MM.yyyy HH:mm}.</p>
                <p>Click on the button below and follow the link to reset a password</p>
                <a href=""{setPasswordUrl}"" style=""display:inline-block; background-color:#4CAF50; color:white; padding:10px 20px; text-decoration:none;"">Reset Password</a>
            </div>";
        try
        {
            await _mailService.SendTextMessageAsync(user.Email, subject, text);
        }
        catch
        {
            throw new ExecutionError($"Could not send an email {user.Email} to reset a password")
            {
                Code = GraphQLCustomErrorCodesEnum.SEND_EMAIL_FAILED.ToString()
            };
        }

        await _userRepository.RemovePasswordAsync(user.Id);

        await _userRepository.AddSetPasswordLinkAsync(setPasswordLink, expired.ToUniversalTime(), user.Id);
    }

    public async Task<UserBusinessResponse> GetCurrentUserFromClaimsAsync()
    {
        var userClaims = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).Claims;
        var userIdClaim = userClaims.FirstOrDefault(c => c.Type == "Id");
        var userDataResponse = await _userRepository.GetUserByIdAsync(Guid.Parse(userIdClaim.Value));
        var userBusinessResponse = _mapper.Map<UserBusinessResponse>(userDataResponse);
        return userBusinessResponse;
    }

    private string HashPassword(string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return passwordHash;
    }
}
