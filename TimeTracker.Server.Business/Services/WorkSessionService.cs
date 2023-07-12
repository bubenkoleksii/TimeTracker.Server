using AutoMapper;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Data.Abstractions;

namespace TimeTracker.Server.Business.Services
{
    public class WorkSessionService : IWorkSessionService
    {
        private readonly IMapper _mapper;
        private readonly IWorkSessionRepository _workSessionRepository;

        public WorkSessionService(IMapper mapper, IWorkSessionRepository workSessionRepository)
        {
            _mapper = mapper;
            _workSessionRepository = workSessionRepository;
        }
    }
}