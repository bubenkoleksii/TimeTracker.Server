﻿namespace TimeTracker.Server.Models.Vacation;

public class VacationApproveRequest
{
    public Guid Id { get; set; }

    public bool IsApproved { get; set; }

    public Guid ApproverId { get; set; }

    public string? ApproverComment { get; set; }
}