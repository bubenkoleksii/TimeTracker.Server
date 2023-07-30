using FluentMigrator;
using TimeTracker.Server.Data.Models.Holidays;

namespace TimeTracker.Server.Data.Migrations;

[Migration(20230730113600)]
public class Migration_20230730113600 : Migration
{
    public override void Up()
    {
        Create.Table("Holidays")
            .WithColumn(nameof(HolidayDataResponse.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(HolidayDataResponse.Title)).AsString().NotNullable()
            .WithColumn(nameof(HolidayDataResponse.Type)).AsString().NotNullable()
            .WithColumn(nameof(HolidayDataResponse.Date)).AsDate().NotNullable()
            .WithColumn(nameof(HolidayDataResponse.EndDate)).AsDate().Nullable();

        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "New Year",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 2)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Christmas",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 1, 7),
            EndDate = new DateTime(2023, 1, 9)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Ukraine Unity Day",
            Type = HolidayTypesEnum.ShortDay.ToString(),
            Date = new DateTime(2023, 1, 22)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Women's day",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 3, 8)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Easter",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 4, 16),
            EndDate = new DateTime(2023, 4, 17)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Labour Day",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 5, 1)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Victory Day over Nazism in World War II in English",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 5, 9)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Day of the Holy Trinity",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 5, 4),
            EndDate = new DateTime(2023, 5, 5)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Ukraine Constitution Day",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 6, 28)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "The Day of Ukrainian statehood",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 7, 28)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "National Flag Day of Ukraine",
            Type = HolidayTypesEnum.ShortDay.ToString(),
            Date = new DateTime(2023, 8, 23)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Independence Day of Ukraine",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 8, 24)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Ukrainian Defenders Day",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 10, 14),
            EndDate = new DateTime(2023, 10, 16)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Day of the Armed Forces of Ukraine",
            Type = HolidayTypesEnum.ShortDay.ToString(),
            Date = new DateTime(2023, 12, 6)
        });
        Insert.IntoTable("Holidays").Row(new
        {
            Id = Guid.NewGuid(),
            Title = "Christmas",
            Type = HolidayTypesEnum.Holiday.ToString(),
            Date = new DateTime(2023, 12, 25)
        });
    }

    public override void Down() 
    {
        Delete.Table("Holidays");
    }
}