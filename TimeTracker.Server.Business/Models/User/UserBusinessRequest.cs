﻿namespace TimeTracker.Server.Business.Models.User;

public record UserBusinessRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}