namespace PRN232_FA25_Assignment_G7.Services.DTOs.Auth;

public record RegisterRequest(
    string Username,
    string Password,
    string FullName,
    string Email,
    string Role
);
