using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Repositories.Repositories.Interfaces;

namespace PRN232_FA25_Assignment_G7.Repositories.Repositories.Implementations;

public class SubjectRepository : Repository<Subject>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext context) : base(context) { }

    public Task<Subject?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(s => s.Code == code, ct);
}
