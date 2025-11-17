namespace PRN232_FA25_Assignment_G7.Services.DTOs.Users;

public record CreateUserRequest(
    string Username,
    string Password,
    string FullName,
    string Email,
    string Role
);
