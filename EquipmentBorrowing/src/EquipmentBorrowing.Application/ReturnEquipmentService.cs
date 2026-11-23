using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;

    public ReturnEquipmentService(IBorrowingRepository borrowingRepository) =>
        _borrowingRepository = borrowingRepository;

    public Task<OpResult> ReturnAsync(string borrowingId, CancellationToken cancellationToken = default) =>
        _borrowingRepository.ReturnAsync(
            borrowingId,
            DateOnly.FromDateTime(DateTime.Today),
            cancellationToken);
}