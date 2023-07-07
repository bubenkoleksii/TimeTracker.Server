﻿using AutoMapper;
using TimeTracker.Server.Business.Models.Auth;
using TimeTracker.Server.Business.Models.Track;
using TimeTracker.Server.Business.Models.User;
using TimeTracker.Server.Data.Models.Track;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Business;

public class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        CreateMap<UserBusinessRequest, UserDataRequest>();
        CreateMap<UserDataResponse, UserBusinessResponse>();

        CreateMap<SetPasswordUserBusinessRequest, SetPasswordUserDataRequest>();

        CreateMap<AuthBusinessRequest, AuthTokenClaimsModel>();
        CreateMap<UserDataResponse, AuthTokenClaimsModel>();

        CreateMap<TrackBusinessRequest, TrackDataRequest>();
        CreateMap<TrackDataResponse, TrackBusinessResponse>();
    }
}