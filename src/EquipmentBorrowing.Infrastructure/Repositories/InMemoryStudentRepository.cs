using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<string, Student> _students = new();

    public Task<Student?> GetByIdAsync(string id)
    {
        _students.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }

    public Task SaveAsync(Student student)
    {
        _students[student.Id] = student;
        return Task.CompletedTask;
    }
}
