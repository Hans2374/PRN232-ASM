namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public record UserResponse(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);
