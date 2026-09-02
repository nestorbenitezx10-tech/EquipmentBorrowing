using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(string id);
    Task UpdateAsync(Equipment equipment);
    Task SaveAsync(Equipment equipment);
}
