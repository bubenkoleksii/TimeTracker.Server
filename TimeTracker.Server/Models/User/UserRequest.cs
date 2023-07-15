using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Server.Models.User;

public record UserRequest
{
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;

    [Required]
    public int EmploymentRate { get; set; }

    [Required]
    public DateTime EmploymentDate { get; set; }

    public string? Permissions { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Status { get; set; } = null!;
}