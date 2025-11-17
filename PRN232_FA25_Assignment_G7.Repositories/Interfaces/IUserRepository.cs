using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> ExistsByUsernameAsync(string username);
}
