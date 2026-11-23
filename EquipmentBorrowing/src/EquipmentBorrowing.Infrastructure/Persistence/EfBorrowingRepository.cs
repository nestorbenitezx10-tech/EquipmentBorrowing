using System.Data;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingRepository(EquipmentBorrowingDbContext context) =>
        _context = context;

    public Task<Borrowing?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _context.Borrowings
            .Include(borrowing => borrowing.Student)
            .Include(borrowing => borrowing.Equipment)
            .SingleOrDefaultAsync(borrowing => borrowing.Id == id, cancellationToken);

    public async Task<IEnumerable<Borrowing>> GetActiveBorrowingsByStudentIdAsync(
        string studentId, CancellationToken cancellationToken = default) =>
        await _context.Borrowings.AsNoTracking()
            .Where(borrowing => borrowing.StudentId == studentId &&
                borrowing.Status == BorrowingStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Add(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Update(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync(
        CancellationToken cancellationToken = default) =>
        await GetActiveAsync(cancellationToken);

    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default) =>
        await _context.Borrowings.AsNoTracking()
            .Include(borrowing => borrowing.Student)
            .Include(borrowing => borrowing.Equipment)
            .Where(borrowing => borrowing.Status == BorrowingStatus.Active)
            .OrderBy(borrowing => borrowing.CheckoutDate)
            .ThenBy(borrowing => borrowing.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Borrowing>> GetStudentHistoryAsync(
        string studentId, CancellationToken cancellationToken = default) =>
        await _context.Borrowings.AsNoTracking()
            .Include(borrowing => borrowing.Equipment)
            .Where(borrowing => borrowing.StudentId == studentId)
            .OrderByDescending(borrowing => borrowing.CheckoutDate)
            .ThenBy(borrowing => borrowing.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Borrowing>> GetEquipmentHistoryAsync(
        string equipmentId, CancellationToken cancellationToken = default) =>
        await _context.Borrowings.AsNoTracking()
            .Include(borrowing => borrowing.Student)
            .Where(borrowing => borrowing.EquipmentId == equipmentId)
            .OrderByDescending(borrowing => borrowing.CheckoutDate)
            .ThenBy(borrowing => borrowing.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortBy,
        bool ascending,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Borrowing> query = _context.Borrowings.AsNoTracking()
            .Include(borrowing => borrowing.Student)
            .Include(borrowing => borrowing.Equipment);
        var term = searchText?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            var pattern = $"%{term.ToLowerInvariant().Replace("%", "\\%").Replace("_", "\\_")}%";
            query = query.Where(borrowing =>
                EF.Functions.Like(borrowing.Student.FullName.ToLower(), pattern, "\\") ||
                EF.Functions.Like(borrowing.Student.StudentNumber.ToLower(), pattern, "\\") ||
                EF.Functions.Like(borrowing.Equipment.Name.ToLower(), pattern, "\\"));
        }

        query = statusFilter switch
        {
            BorrowingStatusFilter.Active => query.Where(borrowing =>
                borrowing.Status == BorrowingStatus.Active),
            BorrowingStatusFilter.Returned => query.Where(borrowing =>
                borrowing.Status == BorrowingStatus.Returned),
            _ => query
        };

        query = (sortBy, ascending) switch
        {
            (BorrowingSortField.CheckoutDate, true) =>
                query.OrderBy(borrowing => borrowing.CheckoutDate).ThenBy(borrowing => borrowing.Id),
            (BorrowingSortField.CheckoutDate, false) =>
                query.OrderByDescending(borrowing => borrowing.CheckoutDate).ThenByDescending(borrowing => borrowing.Id),
            (BorrowingSortField.ExpectedReturnDate, true) =>
                query.OrderBy(borrowing => borrowing.ExpectedReturnDate).ThenBy(borrowing => borrowing.Id),
            _ => query.OrderByDescending(borrowing => borrowing.ExpectedReturnDate)
                .ThenByDescending(borrowing => borrowing.Id)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<OpResult> CheckoutAsync(
        string studentId,
        IReadOnlyCollection<string> equipmentIds,
        DateOnly checkoutDate,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        if (equipmentIds.Count == 0)
            return new(OpOutcome.Rejected, "Select at least one item.");
        if (equipmentIds.Distinct(StringComparer.Ordinal).Count() != equipmentIds.Count)
            return new(OpOutcome.Rejected, "Duplicate items are not allowed.");
        if (expectedReturnDate <= checkoutDate)
            return new(OpOutcome.Rejected, "Expected return must be after checkout.");

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var student = await _context.Students.SingleOrDefaultAsync(
            item => item.Id == studentId, cancellationToken);
        if (student is null)
            return new(OpOutcome.NotFound, "Student not found.");
        if (!student.IsEligible)
            return new(OpOutcome.Rejected, "Student is restricted from borrowing.");

        var activeCount = await _context.Borrowings.CountAsync(
            borrowing => borrowing.StudentId == studentId &&
                borrowing.Status == BorrowingStatus.Active,
            cancellationToken);
        if (activeCount + equipmentIds.Count > 3)
            return new(OpOutcome.Rejected, $"Limit is 3 active items; student has {activeCount}.");

        var items = await _context.Equipment
            .Where(item => equipmentIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        if (items.Count != equipmentIds.Count)
            return new(OpOutcome.NotFound, "An item no longer exists.");

        var unavailable = items.Where(item => !item.IsAvailable).Select(item => item.Identifier).ToList();
        if (unavailable.Count > 0)
            return new(OpOutcome.Rejected, "Not available: " + string.Join(", ", unavailable));

        foreach (var item in items)
        {
            var borrowingId = await ReadableIdGenerator.NextAsync(
                _context, "BorrowingIdSequence", "BR-", 5, cancellationToken);
            _context.Borrowings.Add(new Borrowing
            {
                Id = borrowingId,
                StudentId = studentId,
                EquipmentId = item.Id,
                CheckoutDate = checkoutDate,
                ExpectedReturnDate = expectedReturnDate,
                Status = BorrowingStatus.Active
            });
            item.IsAvailable = false;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(OpOutcome.Conflict, "Data changed. Refreshing.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(OpOutcome.Failed, "Checkout failed. No changes were saved.");
        }

        try
        {
            await transaction.CommitAsync(cancellationToken);
            return new(OpOutcome.Success, "Checkout completed.");
        }
        catch (Exception)
        {
            return new(OpOutcome.UnknownOutcome, "The commit result is uncertain. Refresh records before retrying.");
        }
    }

    public async Task<OpResult> ReturnAsync(
        string borrowingId,
        DateOnly returnDate,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var borrowing = await _context.Borrowings
            .Include(item => item.Equipment)
            .SingleOrDefaultAsync(item => item.Id == borrowingId, cancellationToken);
        if (borrowing is null)
            return new(OpOutcome.NotFound, "Borrowing not found.");
        if (borrowing.Status == BorrowingStatus.Returned)
            return new(OpOutcome.Rejected, "Already returned.");

        try
        {
            borrowing.MarkReturned(returnDate);
            borrowing.Equipment.IsAvailable = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (ArgumentException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(OpOutcome.Rejected, "Return date cannot precede checkout.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(OpOutcome.Conflict, "Record changed. Refreshing.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(OpOutcome.Failed, "Return failed. No changes were saved.");
        }

        try
        {
            await transaction.CommitAsync(cancellationToken);
            return new(OpOutcome.Success, "Equipment returned.");
        }
        catch (Exception)
        {
            return new(OpOutcome.UnknownOutcome, "The commit result is uncertain. Refresh records before retrying.");
        }
    }
}
