namespace PRN232_FA25_Assignment_G7.Services.DTOs.Auth;

public record AuthResponse(
    Guid UserId,
    string Username,
    string FullName,
    string Email,
    string Role,
    string Token
);
