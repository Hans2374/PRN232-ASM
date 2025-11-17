using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
