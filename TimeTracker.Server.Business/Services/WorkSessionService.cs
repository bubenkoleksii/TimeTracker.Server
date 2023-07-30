using AutoMapper;
using GraphQL;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared.Exceptions;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server.Business.Services;

public class WorkSessionService : IWorkSessionService
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly IUserRepository _userRepository;

    public WorkSessionService(IMapper mapper, IConfiguration configuration, IWorkSessionRepository workSessionRepository, IUserRepository userRepository)
    {
        _mapper = mapper;
        _configuration = configuration;
        _workSessionRepository = workSessionRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginationBusinessResponse<WorkSessionBusinessResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc, int? offset,
        int? limit, DateTime? filterDate)
    {
        var limitDefault = int.Parse(_configuration.GetSection("Pagination:WorkSessionLimit").Value);

        var validatedOffset = offset is >= 0 ? offset.Value : default;
        var validatedLimit = limit is > 0 ? limit.Value : limitDefault;

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        var workSessionPaginationDataResponse = await _workSessionRepository.GetWorkSessionsByUserIdAsync(userId, orderByDesc, validatedOffset, validatedLimit, filterDate);
        var workSessionPaginationBusinessResponse = _mapper.Map<PaginationBusinessResponse<WorkSessionBusinessResponse>>(workSessionPaginationDataResponse);
        return workSessionPaginationBusinessResponse;
    }

    public async Task<WorkSessionBusinessResponse> GetWorkSessionByIdAsync(Guid id)
    {
        var workSessionDataResponse = await _workSessionRepository.GetWorkSessionByIdAsync(id);
        var workSessionBusinessResponse = _mapper.Map<WorkSessionBusinessResponse>(workSessionDataResponse);
        return workSessionBusinessResponse;
    }

    public async Task<WorkSessionBusinessResponse> GetActiveWorkSessionByUserIdAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        var workSessionDataResponse = await _workSessionRepository.GetActiveWorkSessionByUserIdAsync(userId);
        var workSessionBusinessResponse = _mapper.Map<WorkSessionBusinessResponse>(workSessionDataResponse);
        return workSessionBusinessResponse;
    }

    public async Task<WorkSessionBusinessResponse> CreateWorkSessionAsync(WorkSessionBusinessRequest workSessionBusinessRequest)
    {
        var user = await _userRepository.GetUserByIdAsync(workSessionBusinessRequest.UserId);
        if (user is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }


        if (user.Permissions is null 
            || (PermissionHelper.HasPermit(user.Permissions, "CreateWorkSessions") == false && workSessionBusinessRequest.UserId != user.Id))
        {
            throw new ExecutionError("User does not have access to read other user's work sessions")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        switch (workSessionBusinessRequest.Type)
        {
            case "planned" or "completed" when workSessionBusinessRequest.End == null:
                throw new ExecutionError("End date of work session cannot be null")
                {
                    Code = GraphQLCustomErrorCodesEnum.DATE_NULL.ToString()
                };
            case "active":
            {
                var activeSession = await _workSessionRepository.GetActiveWorkSessionByUserIdAsync(workSessionBusinessRequest.UserId);

                if (activeSession != null)
                {
                    throw new ExecutionError("You already have an active running session")
                    {
                        Code = GraphQLCustomErrorCodesEnum.INVALID_WORK_SESSION_TYPE.ToString()
                    };
                }

                break;
            }
        }

        var workSessionDataRequest = _mapper.Map<WorkSessionDataRequest>(workSessionBusinessRequest);
        var workSessionDataResponse = await _workSessionRepository.CreateWorkSessionAsync(workSessionDataRequest);
        if (workSessionDataResponse is null)
        {
            throw new ExecutionError("Work session was not created")
            {
                Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
            };
        }

        var workSessionBusinessResponse = _mapper.Map<WorkSessionBusinessResponse>(workSessionDataResponse);
        return workSessionBusinessResponse;
    }

    public async Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime)
    {
        var workSession = await _workSessionRepository.GetWorkSessionByIdAsync(id);
        if (workSession is null)
        {
            throw new ExecutionError("This work session doesn't exist")
            {
                Code = GraphQLCustomErrorCodesEnum.WORK_SESSION_NOT_FOUND.ToString()
            };
        }

        var user = await _userRepository.GetUserByIdAsync(workSession.UserId);
        if (user is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        if (workSession.UserId != user.Id)
        {
            throw new ExecutionError("User does not have access to read other user's work sessions")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        await _workSessionRepository.SetWorkSessionEndAsync(id, endDateTime);
    }

    public async Task UpdateWorkSessionAsync(Guid id, WorkSessionBusinessUpdateRequest workSession)
    {
        var workSessionCheck = await _workSessionRepository.GetWorkSessionByIdAsync(id);
        if (workSessionCheck is null)
        {
            throw new ExecutionError("This work session doesn't exist")
            {
                Code = GraphQLCustomErrorCodesEnum.WORK_SESSION_NOT_FOUND.ToString()
            };
        }

        if (workSessionCheck.Type is "planned" or "completed" && workSession.End == null)
        {
            throw new ExecutionError("End date of work session cannot be null")
            {
                Code = GraphQLCustomErrorCodesEnum.DATE_NULL.ToString()
            };
        }

        var user = await _userRepository.GetUserByIdAsync(workSession.UserId);
        if (user is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        if (user.Permissions is null
            || (PermissionHelper.HasPermit(user.Permissions, "UpdateWorkSessions") == false && workSession.UserId != user.Id))
        {
            throw new ExecutionError("User does not have access to read other user's work sessions")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        var workSessionDataRequest = _mapper.Map<WorkSessionDataUpdateRequest>(workSession);
        await _workSessionRepository.UpdateWorkSessionAsync(id, workSessionDataRequest);
    }

    public async Task DeleteWorkSessionAsync(Guid id)
    {
        var workSession = await _workSessionRepository.GetWorkSessionByIdAsync(id);
        if (workSession is null)
        {
            throw new ExecutionError("This work session doesn't exist")
            {
                Code = GraphQLCustomErrorCodesEnum.WORK_SESSION_NOT_FOUND.ToString()
            };
        }

        if (workSession.End is null)
        {
            throw new ExecutionError("Can not delete active work session")
            {
                Code = GraphQLCustomErrorCodesEnum.WORK_SESSION_IS_ACTIVE.ToString()
            };
        }

        var user = await _userRepository.GetUserByIdAsync(workSession.UserId);
        if (user is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        if (user.Permissions is null
            || (PermissionHelper.HasPermit(user.Permissions, "DeleteWorkSessions") == false && workSession.UserId != user.Id))
        {
            throw new ExecutionError("User does not have access to read other user's work sessions")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        await _workSessionRepository.DeleteWorkSessionAsync(id);
    }
}