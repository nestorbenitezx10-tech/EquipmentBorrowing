using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<(bool Success, string Message)> ReturnAsync(string borrowingId)
    {
        // 1. Locate the borrowing
        var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (borrowing is null)
            return (false, "Borrowing record not found.");

        // 2. Check if it can be returned
        if (borrowing.Status == BorrowingStatus.Returned)
            return (false, "This borrowing has already been returned.");

        // 3. Update status
        borrowing.Status = BorrowingStatus.Returned;
        await _borrowingRepository.UpdateAsync(borrowing);

        // 4. Make equipment available again
        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId);
        if (equipment is not null)
        {
            equipment.IsAvailable = true;
            await _equipmentRepository.UpdateAsync(equipment);
        }

        return (true, "Equipment returned successfully.");
    }
}