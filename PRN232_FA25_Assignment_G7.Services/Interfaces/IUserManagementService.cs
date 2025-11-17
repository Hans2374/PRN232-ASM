using PRN232_FA25_Assignment_G7.Services.DTOs.Users;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IUserManagementService
{
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
}
