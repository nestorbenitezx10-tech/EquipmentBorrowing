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
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<Borrowing> RequestBorrowingAsync(
        string studentId,
        string equipmentId,
        int borrowingDays)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student is null || !student.IsActive)
            throw new InvalidOperationException("Student is not found or inactive.");

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
        if (equipment is null || !equipment.IsAvailable)
            throw new InvalidOperationException("Equipment is not found or unavailable.");

        var borrowing = new Borrowing
        {
            StudentId = studentId,
            EquipmentId = equipmentId,
            ExpectedReturnDate = DateTime.UtcNow.AddDays(borrowingDays),
            Status = BorrowingStatus.Requested
        };

        await _borrowingRepository.SaveAsync(borrowing);
        equipment.IsAvailable = false;
        await _equipmentRepository.UpdateAsync(equipment);

        return borrowing;
    }

    public async Task ConfirmPickupAsync(string borrowingId)
    {
        var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (borrowing is null)
            throw new InvalidOperationException("Borrowing record not found.");

        if (borrowing.Status != BorrowingStatus.Requested)
            throw new InvalidOperationException("Only requested borrowings can be picked up.");

        borrowing.Status = BorrowingStatus.Borrowed;
        borrowing.BorrowedDate = DateTime.UtcNow;
        await _borrowingRepository.UpdateAsync(borrowing);
    }
}