using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.SickLeave;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.SickLeave;
using TimeTracker.Server.Data.Models.WorkSession;
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
            var user = new UserBusinessResponse();

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

            var lastModifier = new UserBusinessResponse();
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

        var workSessionsToDelete = await _workSessionRepository.GetUserWorkSessionsInRangeAsync(sickLeaveDataRequest.UserId,
            sickLeaveDataRequest.Start, sickLeaveDataRequest.End);
        if (workSessionsToDelete.Count > 0)
        {
            await _workSessionRepository.DeleteWorkSessionsAsync(workSessionsToDelete);
        }

        //clear clear place for new sick leave work sessions
        await _workSessionRepository.DeleteWorkSessionsInRangeAsync(sickLeaveDataRequest.UserId, sickLeaveDataRequest.Start, sickLeaveDataRequest.End);

        var user = await _userRepository.GetUserByIdAsync(sickLeaveDataRequest.UserId);

        //create new sick leave work sessions
        await CreateSickLeaveWorkSessionsAsync(sickLeaveDataRequest, user.EmploymentRate);

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

        //delete old sick leave work sessions
        await _workSessionRepository.DeleteWorkSessionsInRangeAsync(oldSickLeave.UserId, oldSickLeave.Start, oldSickLeave.End, WorkSessionStatusEnum.SickLeave);

        //clear clear place for new sick leave work sessions
        await _workSessionRepository.DeleteWorkSessionsInRangeAsync(sickLeaveDataRequest.UserId, sickLeaveDataRequest.Start, sickLeaveDataRequest.End);

        var user = await _userRepository.GetUserByIdAsync(sickLeaveDataRequest.UserId);
        //create new sick leave work sessions
        await CreateSickLeaveWorkSessionsAsync(sickLeaveDataRequest, user.EmploymentRate);

        //set user status
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

            //delete sick leave work sessions
            await _workSessionRepository.DeleteWorkSessionsInRangeAsync(sickLeaveDataResponse.UserId, sickLeaveDataResponse.Start,
                sickLeaveDataResponse.End, WorkSessionStatusEnum.SickLeave);

            var user = await _userRepository.GetUserByIdAsync(sickLeaveDataResponse.UserId);

            //set user status
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

    protected async Task CreateSickLeaveWorkSessionsAsync(SickLeaveDataRequest sickLeaveDataRequest, int employmentRate)
    {
        var sickLeaveDurationInDays = (sickLeaveDataRequest.End - sickLeaveDataRequest.Start).TotalDays + 1;

        var workSessionStart = WorkSessionHelper.GetDefaultWorkSessionStart(sickLeaveDataRequest.Start);
        var workSessionEnd = WorkSessionHelper.GetDefaultWorkSessionEnd(employmentRate, sickLeaveDataRequest.Start);

        var workSessionsToAdd = new List<WorkSessionDataRequest>();
        for (int i = 0; i < sickLeaveDurationInDays; i++)
        {
            var currentWorkSessionStart = workSessionStart.AddDays(i);
            var currentWorkSessionEnd = workSessionEnd.AddDays(i);
            if (WorkSessionHelper.IsNotWeekendDay(currentWorkSessionStart))
            {
                workSessionsToAdd.Add(new WorkSessionDataRequest()
                {
                    UserId = sickLeaveDataRequest.UserId,
                    Start = currentWorkSessionStart,
                    End = currentWorkSessionEnd,
                    Title = null,
                    Description = null,
                    Type = WorkSessionStatusEnum.SickLeave.ToString(),
                    LastModifierId = sickLeaveDataRequest.LastModifierId
                });
            }
        }

        await _workSessionRepository.CreateWorkSessionsAsync(workSessionsToAdd);
    }
}