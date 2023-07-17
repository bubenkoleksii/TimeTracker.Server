using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Server.Models.User;

public record SetPasswordUserRequest
{
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    public string SetPasswordLink { get; set; } = null!;
}