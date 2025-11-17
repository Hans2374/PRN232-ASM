using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public class UpdateUserRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
