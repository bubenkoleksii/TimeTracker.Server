using AutoMapper;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserBusinessResponse> CreateUser(UserBusinessRequest userRequest)
    {
        var candidate = await _userRepository.GetUserByEmail(userRequest.Email);
        if (candidate != null)
            throw new ArgumentNullException($"User with email {userRequest.Email} already exists");

        var userDataRequest = _mapper.Map<UserDataRequest>(userRequest);
        userDataRequest.HashPassword = HashPassword(userRequest.Password);

        var userDataResponse = await _userRepository.CreateUser(userDataRequest);

        var userBusinessResponse = _mapper.Map<UserBusinessResponse>(userDataResponse);
        return userBusinessResponse;
    }

    private string HashPassword(string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return passwordHash;
    }
}