using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.SickLeave;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.SickLeave;
using TimeTracker.Server.Shared;
using TimeTracker.Server.Shared.Exceptions;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server.Business.Services;

public class SickLeaveService : ISickLeaveService
{
    private readonly IMapper _mapper;

    private readonly IUserRepository _userRepository;
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly ISickLeaveRepository _sickLeaveRepository;

    private readonly IUserService _userService;

    public SickLeaveService(IMapper mapper, IUserRepository userRepository, IWorkSessionRepository workSessionRepository,
        ISickLeaveRepository sickLeaveRepository, IUserService userService)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _workSessionRepository = workSessionRepository;
        _sickLeaveRepository = sickLeaveRepository;
        _userService = userService;
    }

    public async Task<List<SickLeaveWithRelationsBusinessResponse>> GetSickLeavesAsync(DateTime date, Guid? userId, bool searchByYear = false)
    {
        var sickLeaveDataResponse = await _sickLeaveRepository.GetSickLeavesAsync(date, userId, searchByYear);
        var sickLeaveBusinessResponse = _mapper.Map<List<SickLeaveBusinessResponse>>(sickLeaveDataResponse);

        var userDict = new Dictionary<Guid, UserBusinessResponse>();

        var sickLeaveWithRelationsList = new List<SickLeaveWithRelationsBusinessResponse>();
        foreach (var sickLeave in sickLeaveBusinessResponse)
        {
            UserBusinessResponse user;

            if (userDict.ContainsKey(sickLeave.UserId))
            {
                user = userDict[sickLeave.UserId];
            }
            else
            {
                var userDataResponse = await _userRepository.GetUserByIdAsync(sickLeave.UserId);
                user = _mapper.Map<UserBusinessResponse>(userDataResponse);

                userDict.Add(user.Id, user);
            }

            UserBusinessResponse lastModifier;
            if (userDict.ContainsKey(sickLeave.LastModifierId))
            {
                lastModifier = userDict[sickLeave.LastModifierId];
            }
            else
            {
                var lastModifierDataResponse = await _userRepository.GetUserByIdAsync(sickLeave.LastModifierId);
                lastModifier = _mapper.Map<UserBusinessResponse>(lastModifierDataResponse);

                userDict.Add(lastModifier.Id, lastModifier);
            }

            sickLeaveWithRelationsList.Add(new SickLeaveWithRelationsBusinessResponse()
            {
                SickLeave = sickLeave,
                User = user,
                LastModifier = lastModifier
            });
        }

        return sickLeaveWithRelationsList;
    }

    public async Task<List<SickLeaveWithRelationsBusinessResponse>> GetUsersSickLeavesForMonthAsync(List<Guid> userIds, DateTime monthDate)
    {
        var startDate = new DateTime(monthDate.Year, monthDate.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(7);
        startDate = startDate.AddDays(-7);

        var sickLeaveDataResponse = await _sickLeaveRepository.GetUsersSickLeaveInRangeAsync(userIds, startDate, endDate);
        var sickLeaveBusinessResponse = _mapper.Map<List<SickLeaveBusinessResponse>>(sickLeaveDataResponse);

        var userDict = new Dictionary<Guid, UserBusinessResponse>();

        var sickLeaveWithRelationsList = new List<SickLeaveWithRelationsBusinessResponse>();
        foreach (var sickLeave in sickLeaveBusinessResponse)
        {
            UserBusinessResponse user;

            if (userDict.ContainsKey(sickLeave.UserId))
            {
                user = userDict[sickLeave.UserId];
            }
            else
            {
                var userDataResponse = await _userRepository.GetUserByIdAsync(sickLeave.UserId);
                user = _mapper.Map<UserBusinessResponse>(userDataResponse);

                userDict.Add(user.Id, user);
            }

            UserBusinessResponse lastModifier;
            if (userDict.ContainsKey(sickLeave.LastModifierId))
            {
                lastModifier = userDict[sickLeave.LastModifierId];
            }
            else
            {
                var lastModifierDataResponse = await _userRepository.GetUserByIdAsync(sickLeave.LastModifierId);
                lastModifier = _mapper.Map<UserBusinessResponse>(lastModifierDataResponse);

                userDict.Add(lastModifier.Id, lastModifier);
            }

            sickLeaveWithRelationsList.Add(new SickLeaveWithRelationsBusinessResponse()
            {
                SickLeave = sickLeave,
                User = user,
                LastModifier = lastModifier
            });
        }

        return sickLeaveWithRelationsList;
    }

    public async Task CreateSickLeaveAsync(SickLeaveBusinessRequest sickLeaveBusinessRequest)
    {
        await ValidateOtherUserEditSickLeave(sickLeaveBusinessRequest.UserId);

        var user = await _userRepository.GetUserByIdAsync(sickLeaveBusinessRequest.UserId);
        if (user.Status != nameof(UserStatusEnum.working))
        {
            throw new ExecutionError("Invalid user status")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_USER_STATUS.ToString()
            };
        }

        if (DateTime.Compare(sickLeaveBusinessRequest.Start, sickLeaveBusinessRequest.End) > 0)
        {
            throw new ExecutionError("Invalid dates input")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_INPUT_DATA.ToString()
            };
        }

        var sickLeaveDataRequest = _mapper.Map<SickLeaveDataRequest>(sickLeaveBusinessRequest);

        //create sick leave data
        await _sickLeaveRepository.CreateSickLeaveAsync(sickLeaveDataRequest);

        //set user status
        if (WorkSessionHelper.IsDateInRange(DateTime.UtcNow, sickLeaveDataRequest.Start, sickLeaveDataRequest.End)
            && user.Status != nameof(UserStatusEnum.ill))
        {
            await _userRepository.SetUserStatusAsync(user.Id, nameof(UserStatusEnum.ill));
        }
    }

    public async Task UpdateSickLeaveAsync(Guid id, SickLeaveBusinessRequest sickLeaveBusinessRequest)
    {
        await ValidateOtherUserEditSickLeave(sickLeaveBusinessRequest.UserId);

        if (DateTime.Compare(sickLeaveBusinessRequest.Start, sickLeaveBusinessRequest.End) > 0)
        {
            throw new ExecutionError("Invalid dates input")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_INPUT_DATA.ToString()
            };
        }

        var oldSickLeave = await _sickLeaveRepository.GetSickLeaveByIdAsync(id);
        if (oldSickLeave is null)
        {
            throw new ExecutionError("Sick leave data not found")
            {
                Code = GraphQLCustomErrorCodesEnum.SICK_LEAVE_NOT_FOUND.ToString()
            };
        }

        var sickLeaveDataRequest = _mapper.Map<SickLeaveDataRequest>(sickLeaveBusinessRequest);
        //update sick leave data
        await _sickLeaveRepository.UpdateSickLeaveAsync(id, sickLeaveDataRequest);

        //set user status
        var user = await _userRepository.GetUserByIdAsync(sickLeaveDataRequest.UserId);
        if (WorkSessionHelper.IsDateInRange(DateTime.UtcNow, sickLeaveDataRequest.Start, sickLeaveDataRequest.End)
            && user.Status != nameof(UserStatusEnum.ill))
        {
            await _userRepository.SetUserStatusAsync(user.Id, nameof(UserStatusEnum.ill));
        }
        else if (!WorkSessionHelper.IsDateInRange(DateTime.UtcNow, sickLeaveDataRequest.Start, sickLeaveDataRequest.End)
                && user.Status == nameof(UserStatusEnum.ill))
        {
            await _userRepository.SetUserStatusAsync(user.Id, nameof(UserStatusEnum.working));
        }
    }

    public async Task DeleteSickLeaveAsync(Guid id)
    {
        var sickLeaveDataResponse = await _sickLeaveRepository.GetSickLeaveByIdAsync(id);
        if (sickLeaveDataResponse is not null)
        {
            await ValidateOtherUserEditSickLeave(sickLeaveDataResponse.UserId);

            await _sickLeaveRepository.DeleteSickLeaveAsync(id);

            //set user status
            var user = await _userRepository.GetUserByIdAsync(sickLeaveDataResponse.UserId);
            if (WorkSessionHelper.IsDateInRange(DateTime.UtcNow, sickLeaveDataResponse.Start, sickLeaveDataResponse.End)
                && user.Status == nameof(UserStatusEnum.ill))
            {
                await _userRepository.SetUserStatusAsync(user.Id, nameof(UserStatusEnum.working));
            }
        }
    }

    protected async Task ValidateOtherUserEditSickLeave(Guid userId)
    {
        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser.Id == userId)
        {
            throw new ExecutionError("You can not start sick leave for yourself")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_USER.ToString()
            };
        }
    }
}