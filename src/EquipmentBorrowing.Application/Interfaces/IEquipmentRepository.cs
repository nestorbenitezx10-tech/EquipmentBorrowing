using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(string id);
    Task<IEnumerable<Equipment>> GetAllAsync();
    Task UpdateAsync(Equipment equipment);
    Task SaveAsync(Equipment equipment);
}
