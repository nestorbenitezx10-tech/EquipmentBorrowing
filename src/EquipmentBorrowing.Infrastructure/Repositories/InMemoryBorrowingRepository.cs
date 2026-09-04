using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly Dictionary<string, Borrowing> _borrowings = new();

    public Task<Borrowing?> GetByIdAsync(string id)
    {
        _borrowings.TryGetValue(id, out var borrowing);
        return Task.FromResult(borrowing);
    }

    public Task<IEnumerable<Borrowing>> GetActiveBorrowingsByStudentIdAsync(string studentId)
    {
        var active = _borrowings.Values.Where(b =>
            b.StudentId == studentId &&
            (b.Status == BorrowingStatus.Requested ||
             b.Status == BorrowingStatus.Borrowed ||
             b.Status == BorrowingStatus.Approved));

        return Task.FromResult(active);
    }

    public Task SaveAsync(Borrowing borrowing)
    {
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Borrowing borrowing)
    {
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    // Added for Lab 2
    public Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        var active = _borrowings.Values.Where(b =>
            b.Status == BorrowingStatus.Requested ||
            b.Status == BorrowingStatus.Borrowed ||
            b.Status == BorrowingStatus.Approved);

        return Task.FromResult(active.AsEnumerable());
    }
}