using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Repositories.Interfaces;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<Subject?> GetByCodeAsync(string code, CancellationToken ct = default);
}
