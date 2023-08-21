﻿namespace TimeTracker.Server.Models.SickLeave;

public class SickLeaveRequest
{
    public Guid UserId { get; set; }

    public Guid LastModifierId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}