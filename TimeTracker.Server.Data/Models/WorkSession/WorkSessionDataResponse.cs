﻿namespace TimeTracker.Server.Data.Models.WorkSession;

public record WorkSessionDataResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public Guid LastModifierId { get; set; }

    public string LastModifierName { get; set; } = null!;
}