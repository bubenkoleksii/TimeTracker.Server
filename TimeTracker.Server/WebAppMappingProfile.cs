using AutoMapper;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Models.Auth;
using TimeTracker.Server.Models.Pagination;
using TimeTracker.Server.Models.WorkSession;
using TimeTracker.Server.Models.User;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Models.Holiday;
using TimeTracker.Server.Business.Models.Holiday;
using TimeTracker.Server.Models.Vacation;
using TimeTracker.Server.Business.Models.Vacation;
using TimeTracker.Server.Models.SickLeave;
using TimeTracker.Server.Business.Models.SickLeave;

namespace TimeTracker.Server;

public class WebAppMappingProfile : Profile
{
    public WebAppMappingProfile()
    {
        CreateMap<AuthRequest, AuthBusinessRequest>();
        CreateMap<AuthBusinessResponse, AuthResponse>();

        CreateMap<UserRequest, UserBusinessRequest>();
        CreateMap<UserBusinessResponse, UserResponse>();
        CreateMap<UserBusinessResponse, ProfileResponse>();
        CreateMap<UserWorkInfoBusinessResponse, UserWorkInfoResponse>();
        CreateMap<PaginationBusinessResponse<UserBusinessResponse>, PaginationResponse<UserResponse>>();
        CreateMap<PaginationBusinessResponse<UserBusinessResponse>, PaginationResponse<ProfileResponse>>();
        CreateMap<PaginationBusinessResponse<UserWorkInfoBusinessResponse>, PaginationResponse<UserWorkInfoResponse>>();

        CreateMap<WorkSessionRequest, WorkSessionBusinessRequest>();
        CreateMap<WorkSessionUpdateRequest, WorkSessionBusinessUpdateRequest>();
        CreateMap<WorkSessionBusinessResponse, WorkSessionResponse>();
        CreateMap<PaginationBusinessResponse<WorkSessionBusinessResponse>, PaginationResponse<WorkSessionResponse>>();
        CreateMap<SetPasswordUserRequest, SetPasswordUserBusinessRequest>();

        CreateMap<HolidayRequest, HolidayBusinessRequest>();
        CreateMap<HolidayBusinessResponse, HolidayResponse>();

        CreateMap<VacationRequest, VacationBusinessRequest>();
        CreateMap<VacationApproveRequest, VacationApproveBusinessRequest>();
        CreateMap<VacationBusinessResponse, VacationResponse>();
        CreateMap<VacationWithUserBusinessResponse, VacationWithUserResponse>();

        CreateMap<VacationInfoBusinessResponse, VacationInfoResponse>();

        CreateMap<SickLeaveRequest, SickLeaveBusinessRequest>();
        CreateMap<SickLeaveBusinessResponse, SickLeaveResponse>();
        CreateMap<SickLeaveWithRelationsBusinessResponse, SickLeaveWithRelationsResponse>();
    }
}
