using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly Dictionary<string, Equipment> _equipmentMap = new();

    public Task<Equipment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _equipmentMap.TryGetValue(id, out var equipment);
        return Task.FromResult(equipment);
    }

    public Task<IReadOnlyList<Equipment>> GetAllAsync(bool availableOnly = false,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Equipment> items = _equipmentMap.Values;
        if (availableOnly)
            items = items.Where(item => item.IsAvailable);
        return Task.FromResult<IReadOnlyList<Equipment>>(
            items.OrderBy(item => item.Identifier).ToList());
    }

    public Task SaveAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipmentMap[equipment.Id] = equipment;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipmentMap[equipment.Id] = equipment;
        return Task.CompletedTask;
    }

    public Task<OpResult> RegisterAsync(string identifier, string name, string? description,
        CancellationToken cancellationToken = default)
    {
        identifier = identifier.Trim();
        name = name.Trim();
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (identifier.Length == 0 || name.Length == 0)
            return Task.FromResult(new OpResult(OpOutcome.Rejected, "Identifier and name are required."));
        if (_equipmentMap.Values.Any(item =>
                Normalizer.Normalize(item.Identifier) == Normalizer.Normalize(identifier)))
            return Task.FromResult(new OpResult(OpOutcome.Conflict, "Equipment identifier already exists."));

        var item = new Equipment
        {
            Id = Guid.NewGuid().ToString(),
            Identifier = identifier,
            IdentifierNormalized = Normalizer.Normalize(identifier),
            Name = name,
            Description = description,
            IsAvailable = true
        };
        _equipmentMap.Add(item.Id, item);
        return Task.FromResult(new OpResult(OpOutcome.Success, "Equipment registered."));
    }

    public Task<OpResult> UpdateAsync(string id, string identifier, string name, string? description,
        CancellationToken cancellationToken = default)
    {
        identifier = identifier.Trim();
        name = name.Trim();
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (identifier.Length == 0 || name.Length == 0)
            return Task.FromResult(new OpResult(OpOutcome.Rejected, "Identifier and name are required."));
        if (!_equipmentMap.TryGetValue(id, out var item))
            return Task.FromResult(new OpResult(OpOutcome.NotFound, "Equipment not found."));
        if (_equipmentMap.Values.Any(other => other.Id != id &&
                Normalizer.Normalize(other.Identifier) == Normalizer.Normalize(identifier)))
            return Task.FromResult(new OpResult(OpOutcome.Conflict, "Equipment identifier already exists."));

        item.Identifier = identifier;
        item.IdentifierNormalized = Normalizer.Normalize(identifier);
        item.Name = name;
        item.Description = description;
        return Task.FromResult(new OpResult(OpOutcome.Success, "Equipment updated."));
    }
}
