using AutoMapper;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Repositories.Interfaces;
using PRN232_FA25_Assignment_G7.Services.DTOs.Users;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserManagementService(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponse>>(users);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        // Check if username already exists
        var exists = await _userRepository.ExistsByUsernameAsync(request.Username);
        if (exists)
        {
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
        }

        // Parse and validate role
        if (!Enum.TryParse<Role>(request.Role, true, out var role))
        {
            throw new ArgumentException($"Invalid role: {request.Role}");
        }

        // Create new user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Role = role,
            IsActive = true
            // CreatedAt will be set by repository
        };

        // Save user
        await _userRepository.AddAsync(user);

        // Return response
        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        // Get existing user
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' not found.");
        }

        // Parse and validate role
        if (!Enum.TryParse<Role>(request.Role, true, out var role))
        {
            throw new ArgumentException($"Invalid role: {request.Role}");
        }

        // Update user properties (do NOT modify password)
        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Role = role;
        user.IsActive = request.IsActive;

        // Save changes
        await _userRepository.UpdateAsync(user);

        // Return response
        return _mapper.Map<UserResponse>(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Get existing user
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        // Soft delete - set IsActive to false
        await _userRepository.DeleteAsync(user);

        return true;
    }
}
