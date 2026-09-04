using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly Dictionary<string, Equipment> _equipmentMap = new();

    public Task<Equipment?> GetByIdAsync(string id)
    {
        _equipmentMap.TryGetValue(id, out var equipment);
        return Task.FromResult(equipment);
    }

    public Task<IEnumerable<Equipment>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Equipment>>(_equipmentMap.Values.ToList());
    }

    public Task SaveAsync(Equipment equipment)
    {
        _equipmentMap[equipment.Id] = equipment;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Equipment equipment)
    {
        _equipmentMap[equipment.Id] = equipment;
        return Task.CompletedTask;
    }
}
