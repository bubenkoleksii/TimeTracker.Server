using FluentMigrator;
using TimeTracker.Server.Data.Models.WorkSession;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Migrations;

[Migration(20230705213700)]
public class Migration_20230705213700 : Migration
{
    public override void Up()
    {
        Create.Table("WorkSession")
            .WithColumn(nameof(WorkSessionDataResponse.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(WorkSessionDataResponse.UserId)).AsGuid().ForeignKey("User", nameof(UserDataResponse.Id))
            .WithColumn(nameof(WorkSessionDataResponse.Start)).AsDateTime().NotNullable()
            .WithColumn(nameof(WorkSessionDataResponse.End)).AsDateTime().Nullable()
            .WithColumn(nameof(WorkSessionDataResponse.Type)).AsString().NotNullable()
            .WithColumn(nameof(WorkSessionDataResponse.Title)).AsString().Nullable()
            .WithColumn(nameof(WorkSessionDataResponse.Description)).AsCustom("TEXT").Nullable();
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Track_User").OnTable("Track");
        Delete.Table("WorkSession");
    }
}