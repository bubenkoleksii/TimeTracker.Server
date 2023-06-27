using AutoMapper;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server;

public class WebAppMappingProfile : Profile
{
    public WebAppMappingProfile()
    {
        CreateMap<UserRequest, UserBusinessRequest>();
        CreateMap<UserBusinessResponse, UserResponse>();
    }
}