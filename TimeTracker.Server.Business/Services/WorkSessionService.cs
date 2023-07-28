using AutoMapper;
using GraphQL;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Shared.Exceptions;

namespace TimeTracker.Server.Business.Services
{
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

            var workSessionPaginationDataResponse = await _workSessionRepository.GetWorkSessionsByUserId(userId, orderByDesc, validatedOffset, validatedLimit, filterDate);
            var workSessionPaginationBusinessResponse = _mapper.Map<PaginationBusinessResponse<WorkSessionBusinessResponse>>(workSessionPaginationDataResponse);
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

            if (workSession.Type.ToLower() == "planned")
            {
                throw new ExecutionError("This work session is planned")
                {
                    Code = GraphQLCustomErrorCodesEnum.INVALID_WORK_SESSION_TYPE.ToString()
                };
            }

            await _workSessionRepository.SetWorkSessionEnd(id, endDateTime);
        }

        public async Task UpdateWorkSessionAsync(Guid id, WorkSessionBusinessUpdateRequest workSession)
        {
            var workSessionCheck = await _workSessionRepository.GetWorkSessionById(id);
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

            var workSessionDataRequest = _mapper.Map<WorkSessionDataUpdateRequest>(workSession);
            await _workSessionRepository.UpdateWorkSession(id, workSessionDataRequest);
        }
    }
}