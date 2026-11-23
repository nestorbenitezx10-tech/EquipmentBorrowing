using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public sealed class BorrowingSearchTests
{
    [Fact]
    public async Task SearchAsync_TrimsCaseInsensitiveTextAndCombinesStatusAndSort()
    {
        var repository = new InMemoryBorrowingRepository();
        var first = CreateBorrowing(
            "B-1",
            "S-1",
            "EQ-1",
            new DateOnly(2026, 11, 10),
            new DateOnly(2026, 11, 20),
            BorrowingStatus.Active,
            "STU-001",
            "Nicolette",
            "Digital Oscilloscope");
        var second = CreateBorrowing(
            "B-2",
            "S-2",
            "EQ-2",
            new DateOnly(2026, 11, 12),
            new DateOnly(2026, 11, 30),
            BorrowingStatus.Active,
            "STU-002",
            "Nestor",
            "Function Generator");
        var returned = CreateBorrowing(
            "B-3",
            "S-1",
            "EQ-1",
            new DateOnly(2026, 11, 9),
            new DateOnly(2026, 11, 15),
            BorrowingStatus.Returned,
            "STU-001",
            "Nicolette",
            "Digital Oscilloscope");
        await repository.SaveAsync(first);
        await repository.SaveAsync(second);
        await repository.SaveAsync(returned);

        var results = await repository.SearchAsync(new BorrowingSearchCriteria(
            " oscillo ",
            BorrowingStatus.Active,
            BorrowingSortField.ExpectedReturnDate,
            SortAscending: false));

        var borrowing = Assert.Single(results);
        Assert.Equal("B-1", borrowing.Id);
    }

    private static Borrowing CreateBorrowing(
        string id,
        string studentId,
        string equipmentId,
        DateOnly checkoutDate,
        DateOnly expectedReturnDate,
        BorrowingStatus status,
        string studentNumber,
        string studentName,
        string equipmentName) =>
        new()
        {
            Id = id,
            StudentId = studentId,
            EquipmentId = equipmentId,
            Student = new Student
            {
                Id = studentId,
                StudentNumber = studentNumber,
                StudentNumberNormalized = Normalizer.Normalize(studentNumber),
                FullName = studentName
            },
            Equipment = new Equipment
            {
                Id = equipmentId,
                Identifier = equipmentId,
                IdentifierNormalized = Normalizer.Normalize(equipmentId),
                Name = equipmentName
            },
            CheckoutDate = checkoutDate,
            ExpectedReturnDate = expectedReturnDate,
            ActualReturnDate = status == BorrowingStatus.Returned ? checkoutDate.AddDays(1) : null,
            Status = status
        };
}
