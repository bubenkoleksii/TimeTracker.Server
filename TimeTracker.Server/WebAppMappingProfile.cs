using AutoMapper;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Models.Auth;
using TimeTracker.Server.Models.User;

namespace TimeTracker.Server;

public class WebAppMappingProfile : Profile
{
    public WebAppMappingProfile()
    {
        CreateMap<AuthRequest, AuthBusinessRequest>();
        CreateMap<AuthBusinessResponse, AuthResponse>();

        CreateMap<UserRequest, UserBusinessRequest>();
        CreateMap<UserBusinessResponse, UserResponse>();
    }
}