using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task<Borrowing?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrowing>> GetActiveBorrowingsByStudentIdAsync(
        string studentId, CancellationToken cancellationToken = default);
    Task SaveAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetStudentHistoryAsync(
        string studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetEquipmentHistoryAsync(
        string equipmentId, CancellationToken cancellationToken = default);
    Task<OpResult> CheckoutAsync(
        string studentId,
        IReadOnlyCollection<string> equipmentIds,
        DateOnly checkoutDate,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken = default);
    Task<OpResult> ReturnAsync(
        string borrowingId,
        DateOnly returnDate,
        CancellationToken cancellationToken = default);
}
