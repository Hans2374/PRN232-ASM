namespace PRN232_FA25_Assignment_G7.Services.DTOs.Auth;

public record RegisterResponse(
    Guid UserId,
    string Username,
    string Role
);
