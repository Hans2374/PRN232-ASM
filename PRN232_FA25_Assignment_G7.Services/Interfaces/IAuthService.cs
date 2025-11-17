using PRN232_FA25_Assignment_G7.Services.DTOs.Auth;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
