using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(string id);
    Task SaveAsync(Student student);
}
