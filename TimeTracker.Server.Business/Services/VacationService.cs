using AutoMapper;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Business.Models.Vacation;
using TimeTracker.Server.Data.Abstractions;
using TimeTracker.Server.Shared.Exceptions;
using TimeTracker.Server.Shared.Helpers;
using TimeTracker.Server.Shared;
using TimeTracker.Server.Data.Models.Vacation;
using TimeTracker.Server.Business.Models.User;

namespace TimeTracker.Server.Business.Services;

public class VacationService : IVacationService
{
    private readonly IMapper _mapper;

    private readonly IVacationInfoRepository _vacationInfoRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly IUserRepository _userRepository;

    private readonly IUserService _userService;

    public VacationService(IMapper mapper, IVacationInfoRepository vacationInfoRepository, IVacationRepository vacationRepository,
        IUserRepository userRepository, IUserService userService)
    {
        _mapper = mapper;
        _vacationInfoRepository = vacationInfoRepository;
        _vacationRepository = vacationRepository;
        _userRepository = userRepository;
        _userService = userService;
    }

    public async Task<IEnumerable<VacationWithUserBusinessResponse>> GetVacationsByUserIdAsync(Guid userId, bool? onlyApproved, bool orderByDesc)
    {
        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (!PermissionHelper.HasPermit(curUser.Permissions, PermissionsEnum.GetVacations))
        {
            if (curUser.Id != userId)
            {
                throw new ExecutionError("User do not has permission to read vacations")
                {
                    Code = GraphQLCustomErrorCodesEnum.NO_PERMISSION.ToString()
                };
            }
        }

        var vacationsDataResponse = await _vacationRepository.GetVacationsByUserIdAsync(userId, onlyApproved, orderByDesc);
        var vacationsBusinessResponse = _mapper.Map<IEnumerable<VacationBusinessResponse>>(vacationsDataResponse);

        var userDataResponse = await _userRepository.GetUserByIdAsync(userId);
        var userBusinessResponse = _mapper.Map<UserBusinessResponse>(userDataResponse);

        var approverDict = new Dictionary<Guid, UserBusinessResponse?>();

        var vacationWithUserBusinessResponses = new List<VacationWithUserBusinessResponse>();
        foreach (var vacation in vacationsBusinessResponse)
        {
            UserBusinessResponse? approver;
            if (vacation.ApproverId is null)
            {
                approver = null;
            }
            else
            {
                if (approverDict.ContainsKey((Guid)vacation.ApproverId))
                {
                    approver = approverDict[(Guid)vacation.ApproverId];
                }
                else
                {
                    var approverDataResponse = await _userRepository.GetUserByIdAsync((Guid)vacation.ApproverId);
                    approver = _mapper.Map<UserBusinessResponse>(approverDataResponse);

                    approverDict.Add(approver.Id, approver);
                }
            }

            vacationWithUserBusinessResponses.Add(new VacationWithUserBusinessResponse()
            {
                Vacation = vacation,
                User = userBusinessResponse,
                Approver = approver
            });
        }

        return vacationWithUserBusinessResponses;
    }

    public async Task<IEnumerable<VacationWithUserBusinessResponse>> GetVacationRequestsAsync(bool getNotStarted)
    {
        var vacationsDataResponse = getNotStarted ?
            await _vacationRepository.GetNotStartedUpdatedVacationsAsync() :
            await _vacationRepository.GetVacationRequestsAsync();

        var vacationsBusinessResponse = _mapper.Map<IEnumerable<VacationBusinessResponse>>(vacationsDataResponse);

        var approverDict = new Dictionary<Guid, UserBusinessResponse?>();

        var vacationWithUserBusinessResponses = new List<VacationWithUserBusinessResponse>();
        foreach (var vacation in vacationsBusinessResponse)
        {
            var userDataResponse = await _userRepository.GetUserByIdAsync(vacation.UserId);
            var user = _mapper.Map<UserBusinessResponse>(userDataResponse);

            UserBusinessResponse? approver;
            if (vacation.ApproverId is null)
            {
                approver = null;
            }
            else
            {
                if (approverDict.ContainsKey((Guid)vacation.ApproverId))
                {
                    approver = approverDict[(Guid)vacation.ApproverId];
                }
                else
                {
                    var approverDataResponse = await _userRepository.GetUserByIdAsync((Guid)vacation.ApproverId);
                    approver = _mapper.Map<UserBusinessResponse>(approverDataResponse);

                    approverDict.Add(approver.Id, approver);
                }
            }

            vacationWithUserBusinessResponses.Add(new VacationWithUserBusinessResponse()
            {
                Vacation = vacation,
                User = user,
                Approver = approver
            });
        }

        return vacationWithUserBusinessResponses;
    }

    public async Task<VacationInfoBusinessResponse> GetVacationInfoByUserIdAsync(Guid userId)
    {
        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (!PermissionHelper.HasPermit(curUser.Permissions, PermissionsEnum.GetVacations))
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
        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser.Id != vacationBusinessRequest.UserId)
        {
            throw new ExecutionError("User can create work session only for himself")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_USER.ToString()
            };
        }

        var user = await _userRepository.GetUserByIdAsync(vacationBusinessRequest.UserId);
        if (user.Status != UserStatusEnum.working.ToString())
        {
            throw new ExecutionError("Invalid user status")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_USER_STATUS.ToString()
            };
        }

        var existingVacationRequest = await _vacationRepository.GetActiveOrNotRespondedVacationUserIdAsync(vacationBusinessRequest.UserId);
        if (existingVacationRequest is not null)
        {
            throw new ExecutionError("User already has a vacation request")
            {
                Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
            };
        }

        if (DateTime.Compare(vacationBusinessRequest.Start, DateTime.UtcNow) <= 0 ||
            DateTime.Compare(vacationBusinessRequest.Start, vacationBusinessRequest.End) > 0)
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
        var vacationDataResponse = await _vacationRepository.GetVacationByIdAsync(vacationApproveBusinessRequest.Id);
        if (vacationDataResponse is null)
        {
            throw new ExecutionError("Vacation not found")
            {
                Code = GraphQLCustomErrorCodesEnum.VACATION_NOT_FOUND.ToString()
            };
        }

        var curUser = await _userService.GetCurrentUserFromClaimsAsync();
        if (curUser.Id == vacationDataResponse.UserId)
        {
            throw new ExecutionError("User can't approve vacation for himself")
            {
                Code = GraphQLCustomErrorCodesEnum.INVALID_USER.ToString()
            };
        }

        if (DateTime.Compare(vacationDataResponse.Start, DateTime.UtcNow) <= 0)
        {
            throw new ExecutionError("Vacation is expired")
            {
                Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
            };
        }

        var vacationApproveDataRequest = _mapper.Map<VacationApproveDataRequest>(vacationApproveBusinessRequest);
        await _vacationRepository.ApproverUpdateVacationAsync(vacationApproveDataRequest);

        var vacationDurationInDays = (vacationDataResponse.End - vacationDataResponse.Start).TotalDays + 1;
        if (vacationDataResponse.IsApproved is null || !(bool)vacationDataResponse.IsApproved)
        {
            if (vacationApproveDataRequest.IsApproved)
            {
                await _vacationInfoRepository.AddDaysSpentAsync(new VacationInfoAddDaysSpendDataRequest()
                {
                    UserId = vacationDataResponse.UserId,
                    DaysSpent = (int)vacationDurationInDays
                });
            }
        }
        else
        {
            if (!vacationApproveDataRequest.IsApproved)
            {
                await _vacationInfoRepository.AddDaysSpentAsync(new VacationInfoAddDaysSpendDataRequest()
                {
                    UserId = vacationDataResponse.UserId,
                    DaysSpent = -(int)vacationDurationInDays
                });
            }
        }
    }

    public async Task DeleteVacationAsync(Guid id)
    {
        var vacationDataResponse = await _vacationRepository.GetVacationByIdAsync(id);
        if (vacationDataResponse is not null)
        {
            var curUser = await _userService.GetCurrentUserFromClaimsAsync();
            if (curUser.Id != vacationDataResponse.UserId)
            {
                throw new ExecutionError("User can create work session only for himself")
                {
                    Code = GraphQLCustomErrorCodesEnum.INVALID_USER.ToString()
                };
            }

            if (vacationDataResponse.IsApproved is not null)
            {
                throw new ExecutionError("Can not delete vacation which was updated by Approver")
                {
                    Code = GraphQLCustomErrorCodesEnum.OPERATION_FAILED.ToString()
                };
            }
            await _vacationRepository.DeleteVacationAsync(id);
        }
    }
}