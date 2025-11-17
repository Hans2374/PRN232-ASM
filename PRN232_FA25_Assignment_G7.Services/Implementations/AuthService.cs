using Microsoft.Extensions.Configuration;
using PRN232_FA25_Assignment_G7.Services.Configuration;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Repositories.Interfaces;
using PRN232_FA25_Assignment_G7.Services.DTOs.Auth;
using PRN232_FA25_Assignment_G7.Services.Helpers;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using JwtTokenGen = PRN232_FA25_Assignment_G7.Services.Helpers.JwtTokenGenerator;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly JwtTokenGen _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        JwtTokenGen jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _jwtTokenGenerator = jwtTokenGenerator;
        
        // Load JWT settings from configuration
        _jwtSettings = new JwtSettings();
        _configuration.GetSection("JwtSettings").Bind(_jwtSettings);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Get user by username
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        // Check if user exists
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is disabled.");
        }

        // Verify password
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user, _jwtSettings);

        // Return auth response
        return new AuthResponse(
            UserId: user.Id,
            Username: user.Username,
            FullName: user.FullName,
            Email: user.Email,
            Role: user.Role.ToString(),
            Token: token
        );
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if username already exists
        var exists = await _userRepository.ExistsByUsernameAsync(request.Username);
        if (exists)
        {
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
        }

        // Parse role
        if (!Enum.TryParse<Role>(request.Role, true, out var role))
        {
            throw new ArgumentException($"Invalid role: {request.Role}");
        }

        // Create new user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Role = role,
            IsActive = true
            // CreatedAt will be set by repository
        };

        // Save user via repository
        await _userRepository.AddAsync(user);

        // Return register response
        return new RegisterResponse(
            UserId: user.Id,
            Username: user.Username,
            Role: user.Role.ToString()
        );
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
