using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.SickLeave;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Business.Models.Vacation;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.SickLeave;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Data.Repositories;
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
        await _sickLeaveRepository.CreateSickLeaveAsync(sickLeaveDataRequest);

        await ReplaceExistingWorkSessionWithSickLeaveWorkSessions(sickLeaveDataRequest);
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

        var sickLeaveDataRequest = _mapper.Map<SickLeaveDataRequest>(sickLeaveBusinessRequest);
        await _sickLeaveRepository.UpdateSickLeaveAsync(id, sickLeaveDataRequest);

        var oldSickLeave = await _sickLeaveRepository.GetSickLeaveByIdAsync(id);
        var oldSickLeaveWorkSessionsToDelete = await _workSessionRepository.GetUserWorkSessionsInRangeAsync(oldSickLeave.UserId, 
            oldSickLeave.Start, oldSickLeave.End, WorkSessionStatusEnum.SickLeave);
        if (oldSickLeaveWorkSessionsToDelete.Count > 0)
        {
            await _workSessionRepository.DeleteWorkSessionsAsync(oldSickLeaveWorkSessionsToDelete);
        }

        await ReplaceExistingWorkSessionWithSickLeaveWorkSessions(sickLeaveDataRequest);
    }

    public async Task DeleteSickLeaveAsync(Guid id)
    {
        var sickLeaveDataRequest = await _sickLeaveRepository.GetSickLeaveByIdAsync(id);
        if (sickLeaveDataRequest is not null)
        {
            await ValidateOtherUserEditSickLeave(sickLeaveDataRequest.UserId);

            await _sickLeaveRepository.DeleteSickLeaveAsync(id);
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

    protected async Task ReplaceExistingWorkSessionWithSickLeaveWorkSessions(SickLeaveDataRequest sickLeaveDataRequest)
    {
        var workSessionsToDelete = await _workSessionRepository.GetUserWorkSessionsInRangeAsync(sickLeaveDataRequest.UserId,
            sickLeaveDataRequest.Start, sickLeaveDataRequest.End);
        if (workSessionsToDelete.Count > 0)
        {
            await _workSessionRepository.DeleteWorkSessionsAsync(workSessionsToDelete);
        }

        var user = await _userRepository.GetUserByIdAsync(sickLeaveDataRequest.UserId);

        var sickLeaveDurationInDays = (sickLeaveDataRequest.End - sickLeaveDataRequest.Start).TotalDays + 1;

        var workSessionStart = WorkSessionHelper.GetDefaultWorkSessionStart();
        var workSessionEnd = WorkSessionHelper.GetDefaultWorkSessionEnd(user.EmploymentRate);

        var workSessionsToAdd = new List<WorkSessionDataRequest>();
        for (int i = 0; i < sickLeaveDurationInDays; i++)
        {
            workSessionsToAdd.Add(new WorkSessionDataRequest()
            {
                UserId = sickLeaveDataRequest.UserId,
                Start = workSessionStart.AddDays(i),
                End = workSessionEnd.AddDays(i),
                Title = null,
                Description = null,
                Type = WorkSessionStatusEnum.SickLeave.ToString(),
                LastModifierId = sickLeaveDataRequest.LastModifierId
            });
        }

        await _workSessionRepository.CreateWorkSessionsAsync(workSessionsToAdd);
    }
}