using GraphQL.Types;
using TimeTracker.Server.Models.Holiday;

namespace TimeTracker.Server.GraphQl.Holiday.Types;

public class HolidayType : ObjectGraphType<HolidayResponse>
{
    public HolidayType()
    {
        Field(h => h.Id, type: typeof(IdGraphType), nullable: false)
                .Description("Id field for holiday object");
        Field(h => h.Title, type: typeof(StringGraphType), nullable: false)
                .Description("Title field for holiday object");
        Field(h => h.Type, type: typeof(StringGraphType), nullable: false)
                .Description("Type field for holiday object");
        Field(h => h.Date, type: typeof(DateGraphType), nullable: false)
                .Description("Date field for holiday object");
        Field(h => h.EndDate, type: typeof(DateGraphType), nullable: true)
                .Description("End date field for holiday object");
    }
}