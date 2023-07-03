﻿using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Server.Models.User;

public record UserRequest
{
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}