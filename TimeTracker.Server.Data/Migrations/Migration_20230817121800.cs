using FluentMigrator;
using TimeTracker.Server.Data.Models.User;
using TimeTracker.Server.Data.Models.SickLeave;

namespace TimeTracker.Server.Data.Migrations;

[Migration(20230817121800)]
public class Migration_20230817121800 : Migration
{
    public override void Up()
    {
        Create.Table("SickLeave")
            .WithColumn(nameof(SickLeaveDataResponse.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(SickLeaveDataResponse.UserId)).AsGuid().ForeignKey("User", nameof(UserDataResponse.Id))
            .WithColumn(nameof(SickLeaveDataResponse.LastModifierId)).AsGuid().NotNullable()
            .WithColumn(nameof(SickLeaveDataResponse.Start)).AsDate().NotNullable()
            .WithColumn(nameof(SickLeaveDataResponse.End)).AsDate().NotNullable();
    }

    public override void Down()
    {
        Delete.ForeignKey(nameof(SickLeaveDataResponse.UserId)).OnTable("User");
        Delete.Table("SickLeave");
    }
}