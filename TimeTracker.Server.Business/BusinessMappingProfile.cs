using AutoMapper;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Business.Models.Holiday;
using TimeTracker.Server.Data.Models.Holidays;
using TimeTracker.Server.Business.Models.Vacation;
using TimeTracker.Server.Data.Models.Vacation;
using TimeTracker.Server.Business.Models.SickLeave;
using TimeTracker.Server.Data.Models.SickLeave;

namespace TimeTracker.Server.Business;

public class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        CreateMap<UserBusinessRequest, UserDataRequest>();
        CreateMap<UserDataResponse, UserBusinessResponse>();
        CreateMap<UserDataResponse, UserWorkInfoBusinessResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
        CreateMap<PaginationDataResponse<UserDataResponse>, PaginationBusinessResponse<UserBusinessResponse>>();

        CreateMap<SetPasswordUserBusinessRequest, SetPasswordUserDataRequest>();

        CreateMap<AuthBusinessRequest, AuthTokenClaimsModel>();
        CreateMap<UserDataResponse, AuthTokenClaimsModel>();

        CreateMap<WorkSessionBusinessRequest, WorkSessionDataRequest>();
        CreateMap<WorkSessionBusinessUpdateRequest, WorkSessionDataUpdateRequest>();
        CreateMap<WorkSessionDataResponse, WorkSessionBusinessResponse>();
        CreateMap<PaginationDataResponse<WorkSessionDataResponse>, PaginationBusinessResponse<WorkSessionBusinessResponse>>();

        CreateMap<HolidayBusinessRequest, HolidayDataRequest>();
        CreateMap<HolidayDataResponse, HolidayBusinessResponse>();

        CreateMap<VacationBusinessRequest, VacationDataRequest>();
        CreateMap<VacationApproveBusinessRequest, VacationApproveDataRequest>();
        CreateMap<VacationDataResponse, VacationBusinessResponse>();

        CreateMap<VacationInfoDataResponse, VacationInfoBusinessResponse>();

        CreateMap<SickLeaveBusinessRequest, SickLeaveDataRequest>();
        CreateMap<SickLeaveDataResponse, SickLeaveBusinessResponse>();
    }
}