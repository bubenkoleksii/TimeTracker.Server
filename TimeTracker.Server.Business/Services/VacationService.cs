using AutoMapper;
using GraphQL;
using Microsoft.AspNetCore.Http;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Vacation;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Shared.Exceptions;
using TimeTracker.Server.Shared.Helpers;
using TimeTracker.Server.Shared;
using TimeTracker.Server.Data.Models.Vacation;

namespace TimeTracker.Server.Business.Services;

public class VacationService : IVacationService
{
    private readonly IMapper _mapper;

    private readonly IVacationInfoRepository _vacationInfoRepository;
    private readonly IVacationRepository _vacationRepository;

    private readonly IUserService _userService;

    public VacationService(IMapper mapper, IVacationInfoRepository vacationInfoRepository, IVacationRepository vacationRepository, IUserService userService)
    {
        _mapper = mapper;
        _vacationInfoRepository = vacationInfoRepository;
        _vacationRepository = vacationRepository;
        _userService = userService;
    }

    public async Task<VacationBusinessResponse> GetVacationByIdAsync(Guid id)
    {
        var vacationDataResponse = await _vacationRepository.GetVacationByIdAsync(id);
        if (vacationDataResponse is null)
        {
            throw new ExecutionError("Vacation not found")
            {
                Code = GraphQLCustomErrorCodesEnum.VACATION_NOT_FOUND.ToString()
            };
        }

        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }
        else if (!PermissionHelper.HasPermit(curUser.Permissions, PermissionsEnum.GetVacation.ToString()))
        {
            if (curUser.Id != vacationDataResponse.UserId)
            {
                throw new ExecutionError("User do not has permission to read this vacation")
                {
                    Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
                };
            }
        }

        var vacationBusinessResponse = _mapper.Map<VacationBusinessResponse>(vacationDataResponse);
        return vacationBusinessResponse;
    }

    public async Task<IEnumerable<VacationBusinessResponse>> GetVacationsByUserIdAsync(Guid userId, bool? onlyApproved, bool orderByDesc)
    {
        var vacationsDataResponse = await _vacationRepository.GetVacationsByUserIdAsync(userId, onlyApproved, orderByDesc);
        var vacationsBusinessResponse = _mapper.Map<IEnumerable<VacationBusinessResponse>>(vacationsDataResponse);
        return vacationsBusinessResponse;
    }

    public async Task<IEnumerable<VacationBusinessResponse>> GetVacationRequestsAsync()
    {
        var vacationsDataResponse = await _vacationRepository.GetVacationRequestsAsync();
        var vacationsBusinessResponse = _mapper.Map<IEnumerable<VacationBusinessResponse>>(vacationsDataResponse);
        return vacationsBusinessResponse;
    }

    public async Task<VacationInfoBusinessResponse> GetVacationInfoByUserIdAsync(Guid userId)
    {
        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser is null)
        {
            throw new ExecutionError("User not found")
            {
                Code = GraphQLCustomErrorCodesEnum.USER_NOT_FOUND.ToString()
            };
        }
        else if (!PermissionHelper.HasPermit(curUser.Permissions, PermissionsEnum.GetVacation.ToString()))
        {
            if (curUser.Id != userId)
            {
                throw new ExecutionError("User do not has permission to read this vacation")
                {
                    Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
                };
            }
        }

        var vacationInfoDataResponse = await _vacationInfoRepository.GetVacationInfoByUserIdAsync(userId);
        var vacationInfoBusinessResponse = _mapper.Map<VacationInfoBusinessResponse>(vacationInfoDataResponse);
        return vacationInfoBusinessResponse;
    }

    public async Task<VacationBusinessResponse> CreateVacationAsync(VacationBusinessRequest vacationBusinessRequest)
    {
        if (DateTime.Compare(vacationBusinessRequest.Start, vacationBusinessRequest.End) > 0)
        {
            throw new ExecutionError("Invalid dates input")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_INPUT_DATA.ToString()
            };
        }

        var vacationDataRequest = _mapper.Map<VacationDataRequest>(vacationBusinessRequest);
        var vacationDataResponse = await _vacationRepository.CreateVacationAsync(vacationDataRequest);
        var vacationBusinessResponse = _mapper.Map<VacationBusinessResponse>(vacationDataResponse);
        return vacationBusinessResponse;
    }

    public async Task ApproverUpdateVacationAsync(VacationApproveBusinessRequest vacationApproveBusinessRequest)
    {
        var vacationApproveDataRequest = _mapper.Map<VacationApproveDataRequest>(vacationApproveBusinessRequest);
        await _vacationRepository.ApproverUpdateVacationAsync(vacationApproveDataRequest);

        if (vacationApproveDataRequest.IsApproved)
        {
            var vacationDataResponse = await _vacationRepository.GetVacationByIdAsync(vacationApproveDataRequest.Id);
            var vacationDurationInDays = (vacationDataResponse.End - vacationDataResponse.Start).TotalDays + 1;
            await _vacationInfoRepository.AddDaysSpentAsync(vacationDataResponse.UserId, (int)vacationDurationInDays);
        }
    }

    public async Task DeleteVacationAsync(Guid id)
    {
        var vacationDataResponse = await _vacationRepository.GetVacationByIdAsync(id);
        if (vacationDataResponse is not null)
        {
            await _vacationRepository.DeleteVacationAsync(id);
        }
    }
}