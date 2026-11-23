using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<string, Student> _students = new();

    public Task<Student?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Student>>(
            _students.Values.OrderBy(student => student.StudentNumber).ToList());

    public Task SaveAsync(Student student, CancellationToken cancellationToken = default)
    {
        _students[student.Id] = student;
        return Task.CompletedTask;
    }

    public Task<OpResult> RegisterAsync(string studentNumber, string fullName, string? contact,
        CancellationToken cancellationToken = default)
    {
        studentNumber = studentNumber.Trim();
        fullName = fullName.Trim();
        contact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim();
        if (studentNumber.Length == 0 || fullName.Length == 0)
            return Task.FromResult(new OpResult(OpOutcome.Rejected, "Student number and name are required."));
        if (_students.Values.Any(student =>
                Normalizer.Normalize(student.StudentNumber) == Normalizer.Normalize(studentNumber)))
            return Task.FromResult(new OpResult(OpOutcome.Conflict, "Student number already exists."));

        var student = new Student
        {
            Id = Guid.NewGuid().ToString(),
            StudentNumber = studentNumber,
            StudentNumberNormalized = Normalizer.Normalize(studentNumber),
            FullName = fullName,
            Contact = contact,
            IsEligible = true
        };
        _students.Add(student.Id, student);
        return Task.FromResult(new OpResult(OpOutcome.Success, "Student registered."));
    }

    public Task<OpResult> UpdateAsync(string id, string studentNumber, string fullName, string? contact,
        CancellationToken cancellationToken = default)
    {
        studentNumber = studentNumber.Trim();
        fullName = fullName.Trim();
        contact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim();
        if (studentNumber.Length == 0 || fullName.Length == 0)
            return Task.FromResult(new OpResult(OpOutcome.Rejected, "Student number and name are required."));
        if (!_students.TryGetValue(id, out var student))
            return Task.FromResult(new OpResult(OpOutcome.NotFound, "Student not found."));
        if (_students.Values.Any(other => other.Id != id &&
                Normalizer.Normalize(other.StudentNumber) == Normalizer.Normalize(studentNumber)))
            return Task.FromResult(new OpResult(OpOutcome.Conflict, "Student number already exists."));

        student.StudentNumber = studentNumber;
        student.StudentNumberNormalized = Normalizer.Normalize(studentNumber);
        student.FullName = fullName;
        student.Contact = contact;
        return Task.FromResult(new OpResult(OpOutcome.Success, "Student updated."));
    }

    public Task<OpResult> SetEligibilityAsync(string id, bool isEligible,
        CancellationToken cancellationToken = default)
    {
        if (!_students.TryGetValue(id, out var student))
            return Task.FromResult(new OpResult(OpOutcome.NotFound, "Student not found."));
        if (student.IsEligible == isEligible)
            return Task.FromResult(new OpResult(OpOutcome.Rejected,
                isEligible ? "Already eligible." : "Already restricted."));

        student.IsEligible = isEligible;
        return Task.FromResult(new OpResult(OpOutcome.Success,
            isEligible ? "Student permitted." : "Student restricted."));
    }
}
