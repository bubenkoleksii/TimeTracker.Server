using FluentMigrator;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Data.Models.Vacation;

namespace TimeTracker.Server.Data.Migrations;

[Migration(20230807111600)]
public class Migration_20230807111600 : Migration
{
    private readonly string _rootUserId;

    public Migration_20230807111600(IConfiguration configuration)
    {
        _rootUserId = configuration["RootUser:Id"];
    }

    public override void Up()
    {
        Create.Table("VacationInfo")
            .WithColumn(nameof(VacationInfoDataResponse.UserId)).AsGuid().ForeignKey("User", nameof(UserDataResponse.Id))
            .WithColumn(nameof(VacationInfoDataResponse.EmploymentDate)).AsDate().NotNullable()
            .WithColumn(nameof(VacationInfoDataResponse.DaysSpent)).AsInt64().NotNullable();

        Insert.IntoTable("VacationInfo").Row(new
        {
            UserId = Guid.Parse(_rootUserId),
            EmploymentDate = new DateTime(2023, 8, 7),
            DaysSpent = 0
        });

        Create.Table("Vacation")
            .WithColumn(nameof(VacationDataResponse.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(VacationDataResponse.UserId)).AsGuid().ForeignKey("User", nameof(UserDataResponse.Id))
            .WithColumn(nameof(VacationDataResponse.Start)).AsDate().NotNullable()
            .WithColumn(nameof(VacationDataResponse.End)).AsDate().NotNullable()
            .WithColumn(nameof(VacationDataResponse.Comment)).AsString().Nullable()
            .WithColumn(nameof(VacationDataResponse.IsApproved)).AsBoolean().Nullable()
            .WithColumn(nameof(VacationDataResponse.ApproverId)).AsGuid().Nullable()
            .WithColumn(nameof(VacationDataResponse.ApproverComment)).AsString().Nullable();
    }

    public override void Down()
    {
        Delete.ForeignKey(nameof(VacationInfoDataResponse.UserId)).OnTable("VacationInfo");
        Delete.ForeignKey(nameof(VacationDataResponse.UserId)).OnTable("Vacation");
        Delete.Table("VacationInfo");
        Delete.Table("Vacation");
    }
}