using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Services;

public class StudentManagementService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public StudentManagementService(
        IStudentRepository studentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
        _borrowingRepository = borrowingRepository ?? throw new ArgumentNullException(nameof(borrowingRepository));
    }

    public Task<IReadOnlyList<Student>> GetAllStudentsAsync(CancellationToken ct = default)
    {
        return _studentRepository.GetAllAsync(ct);
    }

    public Task<Student?> GetStudentByIdAsync(string id, CancellationToken ct = default)
    {
        return _studentRepository.GetByIdAsync(id, ct);
    }

    public Task<OpResult> RegisterStudentAsync(
        string studentNumber,
        string fullName,
        string? contact,
        CancellationToken ct = default)
    {
        return _studentRepository.RegisterAsync(studentNumber, fullName, contact, ct);
    }

    public Task<OpResult> UpdateStudentAsync(
        string id,
        string studentNumber,
        string fullName,
        string? contact,
        CancellationToken ct = default)
    {
        return _studentRepository.UpdateAsync(id, studentNumber, fullName, contact, ct);
    }

    public Task<OpResult> SetEligibilityAsync(
        string id,
        bool isEligible,
        CancellationToken ct = default)
    {
        return _studentRepository.SetEligibilityAsync(id, isEligible, ct);
    }

    public Task<IReadOnlyList<Borrowing>> GetStudentHistoryAsync(
        string studentId,
        CancellationToken ct = default)
    {
        return _borrowingRepository.GetStudentHistoryAsync(studentId, ct);
    }
}