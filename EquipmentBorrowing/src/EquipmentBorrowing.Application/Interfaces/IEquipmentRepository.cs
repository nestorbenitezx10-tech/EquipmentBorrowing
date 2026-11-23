using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Equipment>> GetAllAsync(bool availableOnly = false,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default);
    Task SaveAsync(Equipment equipment, CancellationToken cancellationToken = default);
    Task<OpResult> RegisterAsync(string identifier, string name, string? description,
        CancellationToken cancellationToken = default);
    Task<OpResult> UpdateAsync(string id, string identifier, string name, string? description,
        CancellationToken cancellationToken = default);
}
