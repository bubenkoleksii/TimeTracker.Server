﻿namespace TimeTracker.Server.Business.Models.User;

public record UserWorkInfoBusinessResponse
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int EmploymentRate { get; set; }

    public double WorkedHours { get; set; }

    public double PlannedWorkingHours { get; set; }

    public double SickLeaveHours { get; set; }

    public double VacationHours { get; set; }
}