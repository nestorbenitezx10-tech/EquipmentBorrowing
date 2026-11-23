using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public sealed class MultiItemCheckoutServiceTests
{
    [Fact]
    public async Task CheckoutAsync_RejectsFewerThanTwoItemsWithoutWriting()
    {
        var repository = new RecordingBorrowingRepository();
        var service = CreateService(repository);

        var result = await service.CheckoutAsync(
            "STU-1",
            new[] { "EQ-1" },
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 11, 27));

        Assert.Equal(OpOutcome.Rejected, result.Outcome);
        Assert.Equal(0, repository.CheckoutCalls);
    }

    [Fact]
    public async Task CheckoutAsync_PassesAllSelectedItemsToOneRepositoryOperation()
    {
        var repository = new RecordingBorrowingRepository();
        var service = CreateService(repository);
        var checkoutDate = new DateOnly(2026, 11, 20);
        var returnDate = new DateOnly(2026, 11, 27);

        var result = await service.CheckoutAsync(
            "STU-1",
            new[] { "EQ-1", "EQ-2" },
            checkoutDate,
            returnDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, repository.CheckoutCalls);
        Assert.Equal(new[] { "EQ-1", "EQ-2" }, repository.EquipmentIds);
        Assert.Equal(checkoutDate, repository.CheckoutDate);
        Assert.Equal(returnDate, repository.ExpectedReturnDate);
    }

    [Fact]
    public async Task CheckoutAsync_RejectsDuplicateItemsWithoutWriting()
    {
        var repository = new RecordingBorrowingRepository();
        var service = CreateService(repository);

        var result = await service.CheckoutAsync(
            "STU-1",
            new[] { "EQ-1", "EQ-1" },
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 11, 27));

        Assert.Equal(OpOutcome.Rejected, result.Outcome);
        Assert.Equal(0, repository.CheckoutCalls);
    }

    [Fact]
    public async Task CheckoutAsync_RejectsInvalidDateWithoutWriting()
    {
        var repository = new RecordingBorrowingRepository();
        var service = CreateService(repository);
        var date = new DateOnly(2026, 11, 20);

        var result = await service.CheckoutAsync(
            "STU-1",
            new[] { "EQ-1", "EQ-2" },
            date,
            date);

        Assert.Equal(OpOutcome.Rejected, result.Outcome);
        Assert.Equal(0, repository.CheckoutCalls);
    }

    private static MultiItemCheckoutService CreateService(RecordingBorrowingRepository repository)
    {
        var borrowingService = new BorrowEquipmentService(
            new InMemoryStudentRepository(),
            new InMemoryEquipmentRepository(),
            repository);
        return new MultiItemCheckoutService(borrowingService);
    }

    private sealed class RecordingBorrowingRepository : IBorrowingRepository
    {
        public int CheckoutCalls { get; private set; }
        public IReadOnlyCollection<string> EquipmentIds { get; private set; } = Array.Empty<string>();
        public DateOnly CheckoutDate { get; private set; }
        public DateOnly ExpectedReturnDate { get; private set; }

        public Task<Borrowing?> GetByIdAsync(string id, CancellationToken ct = default) =>
            Task.FromResult<Borrowing?>(null);

        public Task<IEnumerable<Borrowing>> GetActiveBorrowingsByStudentIdAsync(
            string studentId,
            CancellationToken ct = default) =>
            Task.FromResult<IEnumerable<Borrowing>>(Array.Empty<Borrowing>());

        public Task SaveAsync(Borrowing borrowing, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Borrowing borrowing, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync(CancellationToken ct = default) =>
            Task.FromResult<IEnumerable<Borrowing>>(Array.Empty<Borrowing>());

        public Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Borrowing>>(Array.Empty<Borrowing>());

        public Task<IReadOnlyList<Borrowing>> SearchAsync(BorrowingSearchCriteria criteria,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Borrowing>>(Array.Empty<Borrowing>());

        public Task<IReadOnlyList<Borrowing>> GetStudentHistoryAsync(string studentId,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Borrowing>>(Array.Empty<Borrowing>());

        public Task<IReadOnlyList<Borrowing>> GetEquipmentHistoryAsync(string equipmentId,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Borrowing>>(Array.Empty<Borrowing>());

        public Task<OpResult> CheckoutAsync(string studentId, IReadOnlyCollection<string> equipmentIds,
            DateOnly checkoutDate, DateOnly expectedReturnDate, CancellationToken ct = default)
        {
            CheckoutCalls++;
            EquipmentIds = equipmentIds.ToArray();
            CheckoutDate = checkoutDate;
            ExpectedReturnDate = expectedReturnDate;
            return Task.FromResult(new OpResult(OpOutcome.Success, "Checkout completed."));
        }

        public Task<OpResult> ReturnAsync(string borrowingId, DateOnly returnDate,
            CancellationToken ct = default) =>
            Task.FromResult(new OpResult(OpOutcome.Success, "Equipment returned."));
    }
}
