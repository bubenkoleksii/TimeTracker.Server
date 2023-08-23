﻿using AutoMapper;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared;
using TimeTracker.Server.Shared.Exceptions;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server.Business.Services;

public class WorkSessionService : IWorkSessionService
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IUserService _userService;

    public WorkSessionService(IMapper mapper, IConfiguration configuration, IWorkSessionRepository workSessionRepository, IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor, IUserService userService)
    {
        _mapper = mapper;
        _configuration = configuration;
        _workSessionRepository = workSessionRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public async Task<PaginationBusinessResponse<WorkSessionWithRelationsBusinessResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool? orderByDesc, int? offset,
        int? limit, DateTime? startDate, DateTime? endDate)
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

        var workSessionPaginationDataResponse = await _workSessionRepository.GetWorkSessionsByUserIdAsync(userId, orderByDesc, validatedOffset, validatedLimit, startDate, endDate);
        var workSessionPaginationBusinessResponse = _mapper.Map<PaginationBusinessResponse<WorkSessionWithRelationsBusinessResponse>>(workSessionPaginationDataResponse);
        return workSessionPaginationBusinessResponse;
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

        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser is null || curUser.Id != user.Id)
        {
            throw new ExecutionError("Only Owner can get active work session")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_USER.ToString()
            };
        }

        var workSessionDataResponse = await _workSessionRepository.GetActiveWorkSessionByUserIdAsync(userId);

        var workSessionBusinessResponse = _mapper.Map<WorkSessionBusinessResponse>(workSessionDataResponse);
        return workSessionBusinessResponse;
    }

    public async Task<WorkSessionBusinessResponse> CreateWorkSessionAsync(WorkSessionBusinessRequest workSessionBusinessRequest)
    {
        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser is null
            || (!PermissionHelper.HasPermit(curUser.Permissions, PermissionsEnum.CreateWorkSessions)
            && curUser.Id != workSessionBusinessRequest.UserId))
        {
            throw new ExecutionError("User has ho permission for this action")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        var user = await _userRepository.GetUserByIdAsync(workSessionBusinessRequest.UserId);
        if (user is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }

        switch (workSessionBusinessRequest.Type)
        {
            case nameof(WorkSessionStatusEnum.Planned) or nameof(WorkSessionStatusEnum.Completed) when workSessionBusinessRequest.End == null:
                throw new ExecutionError("End date of work session cannot be null")
                {
                    Code = GraphQLCustomErrorCodesEnum.DATE_NULL.ToString()
                };
            case nameof(WorkSessionStatusEnum.Active):
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

        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser is null || curUser.Id != workSession.UserId)
        {
            throw new ExecutionError("User has ho permission for this action")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        await _workSessionRepository.SetWorkSessionEndAsync(id, endDateTime);
    }

    public async Task UpdateWorkSessionAsync(Guid id, WorkSessionBusinessUpdateRequest workSessionBusinessUpdateRequest)
    {
        var workSessionCheck = await _workSessionRepository.GetWorkSessionByIdAsync(id);
        if (workSessionCheck is null)
        {
            throw new ExecutionError("This work session doesn't exist")
            {
                Code = GraphQLCustomErrorCodesEnum.WORK_SESSION_NOT_FOUND.ToString()
            };
        }

        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser is null
            || (!PermissionHelper.HasPermit(curUser.Permissions, PermissionsEnum.UpdateWorkSessions)
            && curUser.Id != workSessionCheck.UserId))
        {
            throw new ExecutionError("User has ho permission for this action")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        var workSessionDataRequest = _mapper.Map<WorkSessionDataUpdateRequest>(workSessionBusinessUpdateRequest);
        await _workSessionRepository.UpdateWorkSessionAsync(id, workSessionDataRequest);
    }

    public async Task DeleteWorkSessionAsync(Guid id)
    {
        var workSession = await _workSessionRepository.GetWorkSessionByIdAsync(id);
        if (workSession is null)
        {
            return;
        }

        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser is null
            || (!PermissionHelper.HasPermit(curUser.Permissions, PermissionsEnum.DeleteWorkSessions)
            && curUser.Id != workSession.UserId))
        {
            throw new ExecutionError("User has ho permission for this action")
            {
                Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
            };
        }

        if (workSession.End is null)
        {
            throw new ExecutionError("Can not delete active work session")
            {
                Code = GraphQLCustomErrorCodesEnum.WORK_SESSION_IS_ACTIVE.ToString()
            };
        }

        await _workSessionRepository.DeleteWorkSessionAsync(id);
    }
}