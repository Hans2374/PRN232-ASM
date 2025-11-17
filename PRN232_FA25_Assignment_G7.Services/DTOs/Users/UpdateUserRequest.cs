namespace PRN232_FA25_Assignment_G7.Services.DTOs.Users;

public record UpdateUserRequest(
    string FullName,
    string Email,
    string Role,
    bool IsActive
);
