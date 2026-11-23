using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(Student student, CancellationToken cancellationToken = default);
    Task<OpResult> RegisterAsync(string studentNumber, string fullName, string? contact,
        CancellationToken cancellationToken = default);
    Task<OpResult> UpdateAsync(string id, string studentNumber, string fullName, string? contact,
        CancellationToken cancellationToken = default);
    Task<OpResult> SetEligibilityAsync(string id, bool isEligible,
        CancellationToken cancellationToken = default);
}
