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

    public UserService(IMailService mailService, IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
    {
        _mailService = mailService;
        _userRepository = userRepository;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<UserBusinessResponse> CreateUser(UserBusinessRequest userRequest)
    {
        var candidate = await _userRepository.GetUserByEmail(userRequest.Email);
        if (candidate != null)
            throw new ArgumentNullException($"User with email {userRequest.Email} already exists");

        var userDataRequest = _mapper.Map<UserDataRequest>(userRequest);
        //userDataRequest.HashPassword = HashPassword(userRequest.Password);

        var userDataResponse = await _userRepository.CreateUser(userDataRequest);

        var userBusinessResponse = _mapper.Map<UserBusinessResponse>(userDataResponse);
        return userBusinessResponse;
    }

    public async Task AddSetPasswordLink(string email)
    {
        var candidate = await _userRepository.GetUserByEmail(email);
        if (candidate == null)
        {
            throw new ExecutionError($"User with email {email} not found")
            {
                Code = "INVALID_EMAIL"
            };
        }

        if (candidate.HasPassword)
        {
            throw new ExecutionError($"User with email {email} already set a password")
            {
                Code = "HAS_PASSWORD"
            };
        }

        var hoursExpired = int.Parse(_configuration.GetSection("Password:HoursExpired").Value);
        var expired = DateTime.UtcNow.AddHours(hoursExpired);

        var setPasswordLink = Guid.NewGuid();
        var setPasswordUrl = $"{_configuration.GetSection("Client:Url").Value}set-password/{setPasswordLink}/";

        var subject = "TimeTracker: Please set a password for your account";
        var text = @$"
            <div>
                <h1>Set Password for Your Account</h1>
                <p>In order to log in to your account, you need to set a password for it before {expired:dd.MM.yyyy HH:mm} UTC.</p>
                <p>Click on the button below and follow the link to set a password</p>
                <a href=""{setPasswordUrl}"" style=""display:inline-block; background-color:#4CAF50; color:white; padding:10px 20px; text-decoration:none;"">Set Password</a>
            </div>";
        try
        {
            await _mailService.SendTextMessage(email, subject, text);
        }
        catch
        {
            throw new ExecutionError($"Could not send an email {email} to set a password")
            {
                Code = "MAIL_SEND_FAILED"
            };
        }

        await _userRepository.AddSetPasswordLink(setPasswordLink, expired, candidate.Id);
    }

    public async Task SetPassword(SetPasswordUserBusinessRequest userRequest)
    {
        var candidate = await _userRepository.GetUserByEmail(userRequest.Email);
        if (candidate == null)
        {
            throw new ExecutionError($"User with email {userRequest.Email} not found")
            {
                Code = "INVALID_EMAIL"
            };
        }

        if (candidate.HasPassword)
        {
            throw new ExecutionError($"User with email {userRequest.Email} has already set a password")
            {
                Code = "PASSWORD_SET"
            };
        }

        if (candidate.SetPasswordLinkExpired < DateTime.UtcNow)
        {
            throw new ExecutionError($"Password link expired for user with email {userRequest.Email}")
            {
                Code = "LINK_EXPIRED"
            };
        }

        if (candidate.SetPasswordLink != new Guid(userRequest.SetPasswordLink))
        {
            throw new ExecutionError($"User with email {userRequest.Email} used the wrong link")
            {
                Code = "LINK_INCORRECT"
            };
        }

        var userDataRequest = _mapper.Map<SetPasswordUserDataRequest>(userRequest);
        userDataRequest.HashPassword = HashPassword(userRequest.Password);

        await _userRepository.SetPassword(userDataRequest);
    }

    private string HashPassword(string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return passwordHash;
    }
}