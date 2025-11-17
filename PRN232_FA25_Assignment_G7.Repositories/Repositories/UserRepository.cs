using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Repositories.Interfaces;

namespace PRN232_FA25_Assignment_G7.Repositories.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        // Soft delete - set IsActive to false instead of hard delete
        user.IsActive = false;
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
}
