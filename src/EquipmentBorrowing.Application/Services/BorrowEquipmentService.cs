using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<Borrowing> RequestBorrowingAsync(string studentId, string equipmentId, int durationDays)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null || !student.IsActive)
        {
            throw new InvalidOperationException("Student is invalid or inactive.");
        }

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
        if (equipment == null || !equipment.IsAvailable)
        {
            throw new InvalidOperationException("Equipment is invalid or unavailable.");
        }

        var activeBorrowings = await _borrowingRepository.GetActiveBorrowingsByStudentIdAsync(studentId);
        if (activeBorrowings.Count() >= 3)
        {
            throw new InvalidOperationException("Student has reached the maximum limit of 3 concurrent items.");
        }

        var borrowing = new Borrowing
        {
            StudentId = studentId,
            EquipmentId = equipmentId,
            ExpectedReturnDate = DateTime.UtcNow.AddDays(durationDays),
            Status = BorrowingStatus.Requested
        };

        await _borrowingRepository.SaveAsync(borrowing);
        return borrowing;
    }

    public async Task ConfirmPickupAsync(string borrowingId)
    {
        var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (borrowing == null || borrowing.Status != BorrowingStatus.Requested)
        {
            throw new InvalidOperationException("Borrowing request is not in a valid state for pickup.");
        }

        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId);
        if (equipment == null) throw new InvalidOperationException("Equipment not found.");

        equipment.IsAvailable = false;
        borrowing.BorrowedDate = DateTime.UtcNow;
        borrowing.Status = BorrowingStatus.Borrowed;

        await _equipmentRepository.UpdateAsync(equipment);
        await _borrowingRepository.UpdateAsync(borrowing);
    }
}
