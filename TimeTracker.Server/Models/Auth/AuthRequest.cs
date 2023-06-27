using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Server.Models.Auth;

public record AuthRequest
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