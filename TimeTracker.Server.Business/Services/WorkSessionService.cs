using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared.Exceptions;
using TimeTracker.Server.Shared.Helpers;

namespace TimeTracker.Server.Business.Services
{
    public class WorkSessionService : IWorkSessionService
    {
        private readonly IMapper _mapper;
        private readonly IWorkSessionRepository _workSessionRepository;
        private readonly IUserRepository _userRepository;

        public WorkSessionService(IMapper mapper, IWorkSessionRepository workSessionRepository, IUserRepository userRepository)
        {
            _mapper = mapper;
            _workSessionRepository = workSessionRepository;
            _userRepository = userRepository;
        }

        public async Task<WorkSessionPaginationBusinessResponse<WorkSessionBusinessResponse>> GetWorkSessionsByUserIdAsync(Guid userId, bool orderByDesc, int offset,
            int limit, DateTime? filterDate)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
            {
                throw new ExecutionError("User not found")
                {
                    Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
                };
            }

            var workSessionPaginationDataResponse = await _workSessionRepository.GetWorkSessionsByUserId(userId, orderByDesc, offset, limit, filterDate);
            var workSessionPaginationBusinessResponse = _mapper.Map<WorkSessionPaginationBusinessResponse<WorkSessionBusinessResponse>>(workSessionPaginationDataResponse);
            return workSessionPaginationBusinessResponse;
        }

        public async Task<WorkSessionBusinessResponse> GetWorkSessionByIdAsync(Guid id)
        {
            var workSessionDataResponse = await _workSessionRepository.GetWorkSessionById(id);
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

            var workSessionDataResponse = await _workSessionRepository.GetActiveWorkSessionByUserId(userId);
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

            var workSessionDataRequest = _mapper.Map<WorkSessionDataRequest>(workSessionBusinessRequest);
            var workSessionDataResponse = await _workSessionRepository.CreateWorkSession(workSessionDataRequest);
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
            var workSession = await _workSessionRepository.GetWorkSessionById(id);
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
            
            if (user.Permissions is null || PermissionHelper.HasPermit(user.Permissions, "ReadWorkSessions"))
            {
                throw new ExecutionError("User does not have access to read other user's work sessions")
                {
                    Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
                };
            }

            await _workSessionRepository.SetWorkSessionEnd(id, endDateTime);
        }

        public async Task UpdateWorkSessionAsync(Guid id, WorkSessionBusinessRequest workSession)
        {
            var workSessionCheck = await _workSessionRepository.GetWorkSessionById(id);
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

            if(true)
            //if (user.Permissions is null || PermissionHelper.HasPermit(user.Permissions, "ReadWorkSessions"))
            {
                throw new ExecutionError("User does not have access to read other user's work sessions")
                {
                    Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
                };
            }

            var workSessionDataRequest = _mapper.Map<WorkSessionDataRequest>(workSession);
            await _workSessionRepository.UpdateWorkSession(id, workSessionDataRequest);
        }
    }
}