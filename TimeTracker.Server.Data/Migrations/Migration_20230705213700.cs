using FluentMigrator;
using TimeTracker.Server.Data.Models.Track;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Migrations;

[Migration(20230705213700)]
public class Migration_20230705213700 : Migration
{
    public override void Up()
    {
        Create.Table("Track")
            .WithColumn(nameof(TrackDataResponse.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(TrackDataResponse.UserId)).AsGuid().ForeignKey("User", nameof(UserDataResponse.Id))
            .WithColumn(nameof(TrackDataResponse.Start)).AsDateTime().NotNullable()
            .WithColumn(nameof(TrackDataResponse.End)).AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Track_User").OnTable("Track");
        Delete.Table("Track");
    }
}