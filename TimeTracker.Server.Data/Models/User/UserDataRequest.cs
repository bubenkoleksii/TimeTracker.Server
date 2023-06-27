﻿namespace TimeTracker.Server.Data.Models.User;

public record UserDataRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? RefreshToken { get; set; }
}