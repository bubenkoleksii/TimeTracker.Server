using System.Security.Claims;
using AutoMapper;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Business.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;

    private readonly IMailService _mailService;

    private readonly IUserRepository _userRepository;

    private readonly IConfiguration _configuration;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IMailService mailService, IUserRepository userRepository, IMapper mapper, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _mailService = mailService;
        _userRepository = userRepository;
        _mapper = mapper;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserBusinessResponse> CreateUserAsync(UserBusinessRequest userRequest)
    {
        var candidate = await _userRepository.GetUserByEmailAsync(userRequest.Email);
        if (candidate != null)
        {
            throw new ExecutionError($"User with email {userRequest.Email} already exists")
            {
                Code = "OPERATION_FAILED"
            };
        }

        var userDataRequest = _mapper.Map<UserDataRequest>(userRequest);

        var userDataResponse = await _userRepository.CreateUserAsync(userDataRequest);

        var userBusinessResponse = _mapper.Map<UserBusinessResponse>(userDataResponse);
        return userBusinessResponse;
    }

    public async Task<IEnumerable<UserBusinessResponse>> GetAllUsersAsync()
    {
        var usersDataResponse = await _userRepository.GetAllUsersAsync();

        var usersBusinessResponse = _mapper.Map<IEnumerable<UserBusinessResponse>>(usersDataResponse);
        return usersBusinessResponse;
    }

    public async Task AddSetPasswordLinkAsync(string email)
    {
        var candidate = await _userRepository.GetUserByEmailAsync(email);
        if (candidate == null)
        {
            throw new ExecutionError($"User with email {email} not found")
            {
                Code = "OPERATION_FAILED"
            };
        }

        if (candidate.HasPassword)
        {
            throw new ExecutionError($"User with email {email} already set a password")
            {
                Code = "OPERATION_FAILED"
            };
        }

        if (candidate.SetPasswordLink != null && candidate.SetPasswordLinkExpired > DateTime.UtcNow)
        {
            throw new ExecutionError($"For user with email {email} already set a password link")
            {
                Code = "OPERATION_FAILED"
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
                Code = "OPERATION_FAILED"
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
                Code = "OPERATION_FAILED"
            };
        }

        if (candidate.HasPassword)
        {
            throw new ExecutionError($"User with email {userRequest.Email} has already set a password")
            {
                Code = "OPERATION_FAILED"
            };
        }

        if (candidate.SetPasswordLinkExpired < DateTime.UtcNow)
        {
            throw new ExecutionError($"Password link expired for user with email {userRequest.Email}")
            {
                Code = "OPERATION_FAILED"
            };
        }

        if (candidate.SetPasswordLink != new Guid(userRequest.SetPasswordLink))
        {
            throw new ExecutionError($"User with email {userRequest.Email} used the wrong link")
            {
                Code = "OPERATION_FAILED"
            };
        }

        var userDataRequest = _mapper.Map<SetPasswordUserDataRequest>(userRequest);
        userDataRequest.HashPassword = HashPassword(userRequest.Password);

        await _userRepository.SetPasswordAsync(userDataRequest);
    }

    public async Task ResetPasswordAsync()
    {
        var claims = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).Claims;

        var userId = claims.FirstOrDefault(c => c.Type == "Id");
        if (userId == null)
        {
            var error = new ExecutionError("Claim user id not found")
            {
                Code = "OPERATION_FAILED"
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
                Code = "OPERATION_FAILED"
            };
        }

        await _userRepository.RemovePasswordAsync(user.Id);

        await _userRepository.AddSetPasswordLinkAsync(setPasswordLink, expired.ToUniversalTime(), user.Id);
    }

    private string HashPassword(string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return passwordHash;
    }
}