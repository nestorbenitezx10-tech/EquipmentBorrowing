using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Services;

public class EquipmentManagementService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public EquipmentManagementService(
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        _borrowingRepository = borrowingRepository ?? throw new ArgumentNullException(nameof(borrowingRepository));
    }

    public Task<IReadOnlyList<Equipment>> GetEquipmentAsync(
        bool availableOnly = false,
        CancellationToken ct = default)
    {
        return _equipmentRepository.GetAllAsync(availableOnly, ct);
    }

    public Task<Equipment?> GetEquipmentByIdAsync(string id, CancellationToken ct = default)
    {
        return _equipmentRepository.GetByIdAsync(id, ct);
    }

    public Task<OpResult> RegisterEquipmentAsync(
        string identifier,
        string name,
        string? description,
        CancellationToken ct = default)
    {
        return _equipmentRepository.RegisterAsync(identifier, name, description, ct);
    }

    public Task<OpResult> UpdateEquipmentAsync(
        string id,
        string identifier,
        string name,
        string? description,
        CancellationToken ct = default)
    {
        return _equipmentRepository.UpdateAsync(id, identifier, name, description, ct);
    }

    public Task<IReadOnlyList<Borrowing>> GetEquipmentHistoryAsync(
        string equipmentId,
        CancellationToken ct = default)
    {
        return _borrowingRepository.GetEquipmentHistoryAsync(equipmentId, ct);
    }
}