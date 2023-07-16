using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Data.Models.WorkSession;

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

        public async Task<IEnumerable<WorkSessionBusinessResponse>> GetWorkSessionsByUserId(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user is null)
                {
                    throw new Exception();
                }

                var workSessionsDataResponse = await _workSessionRepository.GetWorkSessionsByUserId(userId);
                var workSessionsBusinessResponse = _mapper.Map<IEnumerable<WorkSessionBusinessResponse>>(workSessionsDataResponse);
                return workSessionsBusinessResponse;
            }
            catch
            {
                var error = new ExecutionError("Get works sessions by user id operation failed")
                {
                    Code = "OPERATION_FAILED"
                };
                throw error;
            }
        }

        public async Task<WorkSessionBusinessResponse> GetWorkSessionById(Guid id)
        {
            try
            {
                var workSessionDataResponse = await _workSessionRepository.GetWorkSessionById(id);
                var workSessionBusinessResponse = _mapper.Map<WorkSessionBusinessResponse>(workSessionDataResponse);
                return workSessionBusinessResponse;
            }
            catch
            {
                var error = new ExecutionError("Get work session by id operation failed")
                {
                    Code = "OPERATION_FAILED"
                };
                throw error;
            }
        }

        public async Task<WorkSessionBusinessResponse> GetActiveWorkSessionByUserId(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user is null)
                {
                    throw new Exception();
                }

                var workSessionDataResponse = await _workSessionRepository.GetActiveWorkSessionByUserId(userId);
                var workSessionBusinessResponse = _mapper.Map<WorkSessionBusinessResponse>(workSessionDataResponse);
                return workSessionBusinessResponse;
            }
            catch
            {
                var error = new ExecutionError("Get active work session by user id operation failed")
                {
                    Code = "OPERATION_FAILED"
                };
                throw error;
            }
        }

        public async Task<WorkSessionBusinessResponse> CreateWorkSessionAsync(WorkSessionBusinessRequest workSessionBusinessRequest)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(workSessionBusinessRequest.UserId);
                if (user is null)
                {
                    throw new Exception();
                }

                var workSessionDataRequest = _mapper.Map<WorkSessionDataRequest>(workSessionBusinessRequest);
                var workSessionDataResponse = await _workSessionRepository.CreateWorkSession(workSessionDataRequest);
                if (workSessionDataResponse is null)
                {
                    throw new Exception();
                }

                var workSessionBusinessResponse = _mapper.Map<WorkSessionBusinessResponse>(workSessionDataResponse);
                return workSessionBusinessResponse;
            }
            catch
            {
                var error = new ExecutionError("Work session create operation failed")
                {
                    Code = "OPERATION_FAILED"
                };
                throw error;
            }
        }

        public async Task SetWorkSessionEndAsync(Guid id, DateTime endDateTime)
        {
            try
            {
                var workSession = await _workSessionRepository.GetWorkSessionById(id);
                if (workSession is null)
                {
                    throw new Exception();
                }

                await _workSessionRepository.SetWorkSessionEnd(id, endDateTime);
            }
            catch
            {
                var error = new ExecutionError("Set work session end datetime operation failed")
                {
                    Code = "OPERATION_FAILED"
                };
                throw error;
            }
        }
    }
}