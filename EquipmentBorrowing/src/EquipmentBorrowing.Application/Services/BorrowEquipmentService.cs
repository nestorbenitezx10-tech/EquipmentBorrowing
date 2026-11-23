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
            CheckoutDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedReturnDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(borrowingDays),
            Status = BorrowingStatus.Requested
        };

        await _borrowingRepository.SaveAsync(borrowing);
        equipment.IsAvailable = false;
        await _equipmentRepository.UpdateAsync(equipment);

        return borrowing;
    }

    public Task<OpResult> RequestCheckoutAsync(
        string studentId,
        IReadOnlyCollection<string> equipmentIds,
        DateOnly checkoutDate,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken = default) =>
        _borrowingRepository.CheckoutAsync(
            studentId,
            equipmentIds,
            checkoutDate,
            expectedReturnDate,
            cancellationToken);

    public async Task<OpResult> RequestMultiBorrowingAsync(
        string studentId,
        IReadOnlyCollection<string> equipmentIds,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return new(OpOutcome.Rejected, "Student ID is required.");
        if (equipmentIds.Count < 2)
            return new(OpOutcome.Rejected, "Multi-item checkout requires at least two equipment items.");
        if (equipmentIds.Distinct(StringComparer.Ordinal).Count() != equipmentIds.Count)
            return new(OpOutcome.Rejected, "Duplicate equipment items are not allowed.");

        var checkoutDate = DateOnly.FromDateTime(DateTime.Today);
        if (expectedReturnDate <= checkoutDate)
            return new(OpOutcome.Rejected, "Expected return date must be after checkout.");

        return await RequestCheckoutAsync(
            studentId.Trim(),
            equipmentIds,
            checkoutDate,
            expectedReturnDate,
            cancellationToken);
    }

    public Task<IReadOnlyList<Borrowing>> SearchBorrowingsAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken cancellationToken = default) =>
        _borrowingRepository.SearchAsync(
            searchText, statusFilter, sortBy, ascending, cancellationToken);

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