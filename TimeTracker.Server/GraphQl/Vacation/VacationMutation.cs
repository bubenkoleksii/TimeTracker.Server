using AutoMapper;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using GraphQL;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Shared;
using TimeTracker.Server.GraphQl.Vacation.Types;
using TimeTracker.Server.Models.Vacation;
using TimeTracker.Server.Business.Models.Vacation;

namespace TimeTracker.Server.GraphQl.Vacation;

public class VacationMutation : ObjectGraphType
{
    public VacationMutation(IMapper mapper)
    {
        Field<BooleanGraphType>("createVacation")
                .Argument<NonNullGraphType<VacationInputType>>("vacation")
                .Resolve()
                .WithScope()
                .WithService<IVacationService>()
                .ResolveAsync(async (context, service) =>
                {
                    var vacation = context.GetArgument<VacationRequest>("vacation");

                    var vacationBusinessRequest = mapper.Map<VacationBusinessRequest>(vacation);
                    await service.CreateVacationAsync(vacationBusinessRequest);

                    return true;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());

        Field<BooleanGraphType>("updateByApprover")
                .Argument<NonNullGraphType<VacationApproveInputType>>("approverUpdate")
                .Resolve()
                .WithScope()
                .WithService<IVacationService>()
                .ResolveAsync(async (context, service) =>
                {
                    var vacationApproveRequest = context.GetArgument<VacationApproveRequest>("approverUpdate");

                    var vacationApproveBusinessRequest = mapper.Map<VacationApproveBusinessRequest>(vacationApproveRequest);
                    await service.ApproverUpdateVacationAsync(vacationApproveBusinessRequest);

                    return true;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString())
                  .AuthorizeWithPolicy(PermissionsEnum.ApproveVacations.ToString());

        Field<BooleanGraphType>("delete")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Resolve()
                .WithScope()
                .WithService<IVacationService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");

                    await service.DeleteVacationAsync(id);

                    return true;
                }).AuthorizeWithPolicy(PermissionsEnum.LoggedIn.ToString());
    }
}