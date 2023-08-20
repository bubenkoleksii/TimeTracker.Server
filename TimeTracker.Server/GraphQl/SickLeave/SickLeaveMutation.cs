using GraphQL;
using GraphQL.Types;
using GraphQL.MicrosoftDI;
using TimeTracker.Server.Business.Abstractions;
using TimeTracker.Server.Shared;
using AutoMapper;
using TimeTracker.Server.GraphQl.SickLeave.Types;
using TimeTracker.Server.Models.SickLeave;
using TimeTracker.Server.Business.Models.SickLeave;

namespace TimeTracker.Server.GraphQl.SickLeave;

public class SickLeaveMutation : ObjectGraphType
{
    public SickLeaveMutation(IMapper mapper)
    {
        Field<BooleanGraphType>("create")
                .Argument<NonNullGraphType<SickLeaveInputType>>("sickLeave")
                .Resolve()
                .WithScope()
                .WithService<ISickLeaveService>()
                .ResolveAsync(async (context, service) =>
                {
                    var sickLeave = context.GetArgument<SickLeaveRequest>("sickLeave");

                    var sickLeaveBusinessRequest = mapper.Map<SickLeaveBusinessRequest>(sickLeave);
                    await service.CreateSickLeaveAsync(sickLeaveBusinessRequest);

                    return true;
                }).AuthorizeWithPolicy(nameof(PermissionsEnum.LoggedIn))
                  .AuthorizeWithPolicy(nameof(PermissionsEnum.ManageSickLeaves));

        Field<BooleanGraphType>("update")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Argument<NonNullGraphType<SickLeaveInputType>>("sickLeave")
                .Resolve()
                .WithScope()
                .WithService<ISickLeaveService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");
                    var sickLeave = context.GetArgument<SickLeaveRequest>("sickLeave");

                    var sickLeaveBusinessRequest = mapper.Map<SickLeaveBusinessRequest>(sickLeave);
                    await service.UpdateSickLeaveAsync(id, sickLeaveBusinessRequest);

                    return true;
                }).AuthorizeWithPolicy(nameof(PermissionsEnum.LoggedIn))
                  .AuthorizeWithPolicy(nameof(PermissionsEnum.ManageSickLeaves));

        Field<BooleanGraphType>("delete")
                .Argument<NonNullGraphType<IdGraphType>>("id")
                .Resolve()
                .WithScope()
                .WithService<ISickLeaveService>()
                .ResolveAsync(async (context, service) =>
                {
                    var id = context.GetArgument<Guid>("id");

                    await service.DeleteSickLeaveAsync(id);

                    return true;
                }).AuthorizeWithPolicy(nameof(PermissionsEnum.LoggedIn))
                  .AuthorizeWithPolicy(nameof(PermissionsEnum.ManageSickLeaves));
    }
}