using AutoMapper;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Business;

public class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        CreateMap<UserBusinessRequest, UserDataRequest>();
    }
}