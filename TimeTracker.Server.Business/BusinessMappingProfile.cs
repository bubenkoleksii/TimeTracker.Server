using AutoMapper;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Business.Models.WorkSession;
using TimeTracker.Server.Business.Models.Pagination;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Data.Models.Pagination;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Business;

public class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        CreateMap<UserBusinessRequest, UserDataRequest>();
        CreateMap<UserDataResponse, UserBusinessResponse>();
        CreateMap<PaginationDataResponse<UserDataResponse>, PaginationBusinessResponse<UserBusinessResponse>>();

        CreateMap<SetPasswordUserBusinessRequest, SetPasswordUserDataRequest>();

        CreateMap<AuthBusinessRequest, AuthTokenClaimsModel>();
        CreateMap<UserDataResponse, AuthTokenClaimsModel>();

        CreateMap<WorkSessionBusinessRequest, WorkSessionDataRequest>();
        CreateMap<WorkSessionDataResponse, WorkSessionBusinessResponse>();
        CreateMap<WorkSessionPaginationDataResponse<WorkSessionDataResponse>, WorkSessionPaginationBusinessResponse<WorkSessionBusinessResponse>>();
    }
}