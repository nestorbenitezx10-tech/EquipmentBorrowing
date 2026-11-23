using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfEquipmentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipment.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(bool availableOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Equipment.AsNoTracking();
        if (availableOnly)
            query = query.Where(item => item.IsAvailable);
        return await query.OrderBy(item => item.Identifier).ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await _context.Equipment.AddAsync(equipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _context.Equipment.Update(equipment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OpResult> RegisterAsync(string identifier, string name, string? description,
        CancellationToken cancellationToken = default)
    {
        identifier = identifier.Trim();
        name = name.Trim();
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (identifier.Length == 0 || name.Length == 0)
            return new(OpOutcome.Rejected, "Identifier and name are required.");
        var normalized = Normalizer.Normalize(identifier);
        if (await _context.Equipment.AnyAsync(
                item => item.IdentifierNormalized == normalized,
                cancellationToken))
            return new(OpOutcome.Conflict, "Equipment identifier already exists.");

        _context.Equipment.Add(new Equipment
        {
            Id = identifier,
            Identifier = identifier,
            IdentifierNormalized = normalized,
            Name = name,
            Description = description,
            IsAvailable = true
        });
        await _context.SaveChangesAsync(cancellationToken);
        return new(OpOutcome.Success, "Equipment registered.");
    }

    public async Task<OpResult> UpdateAsync(string id, string identifier, string name, string? description,
        CancellationToken cancellationToken = default)
    {
        identifier = identifier.Trim();
        name = name.Trim();
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (identifier.Length == 0 || name.Length == 0)
            return new(OpOutcome.Rejected, "Identifier and name are required.");
        var equipment = await _context.Equipment.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (equipment is null)
            return new(OpOutcome.NotFound, "Equipment not found.");
        var normalized = Normalizer.Normalize(identifier);
        if (await _context.Equipment.AnyAsync(
                item => item.Id != id && item.IdentifierNormalized == normalized,
                cancellationToken))
            return new(OpOutcome.Conflict, "Equipment identifier already exists.");

        equipment.Identifier = identifier;
        equipment.IdentifierNormalized = normalized;
        equipment.Name = name;
        equipment.Description = description;
        await _context.SaveChangesAsync(cancellationToken);
        return new(OpOutcome.Success, "Equipment updated.");
    }
}
