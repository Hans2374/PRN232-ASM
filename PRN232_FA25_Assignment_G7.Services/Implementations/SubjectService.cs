using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Repositories.Repositories.Interfaces;
using PRN232_FA25_Assignment_G7.Services.DTOs.Subject;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repository;

    public SubjectService(ISubjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<SubjectResponse?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _repository.GetByIdAsync(id, ct);
        return subject == null ? null : new SubjectResponse(subject.Id, subject.Code, subject.Name);
    }

    public async Task<IReadOnlyList<SubjectResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var subjects = await _repository.ListAsync(ct: ct);
        return subjects.Select(s => new SubjectResponse(s.Id, s.Code, s.Name)).ToList();
    }

    public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByCodeAsync(request.Code, ct);
        if (existing != null)
            throw new InvalidOperationException($"Subject with code '{request.Code}' already exists.");

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name
        };

        await _repository.AddAsync(subject, ct);
        await _repository.SaveChangesAsync(ct);

        return new SubjectResponse(subject.Id, subject.Code, subject.Name);
    }

    public async Task<SubjectResponse?> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await _repository.GetByIdAsync(id, ct);
        if (subject == null) return null;

        subject.Code = request.Code;
        subject.Name = request.Name;

        _repository.Update(subject);
        await _repository.SaveChangesAsync(ct);

        return new SubjectResponse(subject.Id, subject.Code, subject.Name);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _repository.GetByIdAsync(id, ct);
        if (subject == null) return false;

        _repository.Remove(subject);
        await _repository.SaveChangesAsync(ct);
        return true;
    }
}
