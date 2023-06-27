using FluentMigrator;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Migrations;

[Migration(20230627112400)]
public class Migration_20230627112400 : Migration
{
    public override void Up()
    {
        Create.Table("User")
            .WithColumn(nameof(UserDataResponse.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(UserDataResponse.Email)).AsString(255).Unique().NotNullable()
            .WithColumn(nameof(UserDataResponse.HashPassword)).AsString(255).NotNullable()
            .WithColumn(nameof(UserDataResponse.RefreshToken)).AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete.Table("User");
    }
}