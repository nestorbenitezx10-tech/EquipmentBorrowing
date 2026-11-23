using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly Dictionary<string, Borrowing> _borrowings = new();

    public Task<Borrowing?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _borrowings.TryGetValue(id, out var borrowing);
        return Task.FromResult(borrowing);
    }

    public Task<IEnumerable<Borrowing>> GetActiveBorrowingsByStudentIdAsync(
        string studentId, CancellationToken cancellationToken = default)
    {
        var active = _borrowings.Values.Where(b =>
            b.StudentId == studentId &&
            b.Status == BorrowingStatus.Active);

        return Task.FromResult(active);
    }

    public Task SaveAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        var active = _borrowings.Values.Where(b =>
            b.Status == BorrowingStatus.Active);

        return Task.FromResult(active.AsEnumerable());
    }

    public Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Borrowing>>(
            _borrowings.Values.Where(b => b.Status == BorrowingStatus.Active).ToList());

    public Task<IReadOnlyList<Borrowing>> GetStudentHistoryAsync(string studentId,
        CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Borrowing>>(
            _borrowings.Values.Where(b => b.StudentId == studentId)
                .OrderByDescending(b => b.CheckoutDate).ThenBy(b => b.Id).ToList());

    public Task<IReadOnlyList<Borrowing>> GetEquipmentHistoryAsync(string equipmentId,
        CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Borrowing>>(
            _borrowings.Values.Where(b => b.EquipmentId == equipmentId)
                .OrderByDescending(b => b.CheckoutDate).ThenBy(b => b.Id).ToList());

    public Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken ct = default)
    {
        IEnumerable<Borrowing> query = _borrowings.Values;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(b =>
                (b.Student?.FullName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.Student?.StudentNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.Equipment?.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        query = statusFilter switch
        {
            BorrowingStatusFilter.Active => query.Where(b => b.Status == BorrowingStatus.Active),
            BorrowingStatusFilter.Returned => query.Where(b => b.Status == BorrowingStatus.Returned),
            _ => query
        };

        query = (sortBy, ascending) switch
        {
            (BorrowingSortField.CheckoutDate, true) => query.OrderBy(b => b.CheckoutDate).ThenBy(b => b.Id),
            (BorrowingSortField.CheckoutDate, false) => query.OrderByDescending(b => b.CheckoutDate).ThenByDescending(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, true) => query.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, false) => query.OrderByDescending(b => b.ExpectedReturnDate).ThenByDescending(b => b.Id),
            _ => query.OrderByDescending(b => b.CheckoutDate).ThenByDescending(b => b.Id)
        };

        return Task.FromResult<IReadOnlyList<Borrowing>>(query.ToList());
    }

    public Task<OpResult> CheckoutAsync(string studentId, IReadOnlyCollection<string> equipmentIds,
        DateOnly checkoutDate, DateOnly expectedReturnDate, CancellationToken ct = default)
    {
        if (equipmentIds.Count == 0)
            return Task.FromResult(new OpResult(OpOutcome.Rejected, "Select at least one item."));
        foreach (var equipmentId in equipmentIds)
        {
            _borrowings[Guid.NewGuid().ToString()] = new Borrowing
            {
                StudentId = studentId,
                EquipmentId = equipmentId,
                CheckoutDate = checkoutDate,
                ExpectedReturnDate = expectedReturnDate,
                Status = BorrowingStatus.Active
            };
        }
        return Task.FromResult(new OpResult(OpOutcome.Success, "Checkout completed."));
    }

    public Task<OpResult> ReturnAsync(string borrowingId, DateOnly returnDate,
        CancellationToken ct = default)
    {
        if (!_borrowings.TryGetValue(borrowingId, out var borrowing))
            return Task.FromResult(new OpResult(OpOutcome.NotFound, "Borrowing not found."));
        if (borrowing.Status == BorrowingStatus.Returned)
            return Task.FromResult(new OpResult(OpOutcome.Rejected, "Already returned."));
        borrowing.MarkReturned(returnDate);
        return Task.FromResult(new OpResult(OpOutcome.Success, "Equipment returned."));
    }
}