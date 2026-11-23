using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class MigrationTests
{
    [Fact]
    [Trait("AcceptanceId", "A17")]
    [Trait("UseCaseId", "UC-17")]
    public async Task Upgrade_PreservesLegacyRecordsAndMapsCurrentSchema()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new EquipmentBorrowingDbContext(options);
        var initialMigration = context.Database.GetMigrations().First();
        await context.GetService<IMigrator>().MigrateAsync(initialMigration);

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO Students (Id, Name, Email, IsActive) " +
            "VALUES ('S1', 'Test Student', 'student@example.test', 1);");
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO Equipment (Id, Name, AssetTag, IsAvailable) " +
            "VALUES ('E1', 'Laptop', 'EQ-001', 0);");
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO Borrowings " +
            "(Id, StudentId, EquipmentId, RequestDate, BorrowedDate, ReturnDate, ExpectedReturnDate, Status) " +
            "VALUES " +
            "('B1', 'S1', 'E1', '2026-01-01 00:00:00', '2026-01-01 00:00:00', NULL, " +
            "'2026-01-10 00:00:00', 2), " +
            "('B2', 'S1', 'E1', '2025-12-01 00:00:00', '2025-12-01 00:00:00', " +
            "'2025-12-05 00:00:00', '2025-12-10 00:00:00', 3);");

        await context.Database.MigrateAsync();

        var student = await context.Students.SingleAsync();
        Assert.Equal("Test Student", student.FullName);
        Assert.Equal("student@example.test", student.Contact);
        Assert.Equal("S1", student.StudentNumber);
        Assert.Equal("S1", student.StudentNumberNormalized);

        var equipment = await context.Equipment.SingleAsync();
        Assert.Equal("EQ-001", equipment.Identifier);
        Assert.Equal("EQ-001", equipment.IdentifierNormalized);

        var borrowings = await context.Borrowings.OrderBy(item => item.Id).ToListAsync();
        Assert.Equal(2, borrowings.Count);
        Assert.Equal(BorrowingStatus.Active, borrowings[0].Status);
        Assert.Equal(new DateOnly(2026, 1, 1), borrowings[0].CheckoutDate);
        Assert.Equal(BorrowingStatus.Returned, borrowings[1].Status);
        Assert.Equal(new DateOnly(2025, 12, 5), borrowings[1].ActualReturnDate);

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO Equipment " +
            "(Id, Identifier, IdentifierNormalized, Name, Description, IsAvailable) " +
            "VALUES ('E2', 'EQ-002', 'EQ-002', 'Monitor', NULL, 1);");

        await Assert.ThrowsAsync<SqliteException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "INSERT INTO Students " +
                "(Id, StudentNumber, StudentNumberNormalized, FullName, Contact, IsEligible) " +
                "VALUES ('S2', 'STU-002', 'S1', 'Another Student', NULL, 1);"));
        await Assert.ThrowsAsync<SqliteException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "INSERT INTO Equipment " +
                "(Id, Identifier, IdentifierNormalized, Name, Description, IsAvailable) " +
                "VALUES ('E3', 'EQ-003', 'EQ-001', 'Printer', NULL, 1);"));
        await Assert.ThrowsAsync<SqliteException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "INSERT INTO Borrowings " +
                "(Id, StudentId, EquipmentId, CheckoutDate, ExpectedReturnDate, Status) " +
                "VALUES ('B3', 'missing', 'E2', '2026-01-01', '2026-01-10', 0);"));
        await Assert.ThrowsAsync<SqliteException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "INSERT INTO Borrowings " +
                "(Id, StudentId, EquipmentId, CheckoutDate, ExpectedReturnDate, Status) " +
                "VALUES ('B4', 'S1', 'E2', '2026-01-10', '2026-01-10', 0);"));
        await Assert.ThrowsAsync<SqliteException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "INSERT INTO Borrowings " +
                "(Id, StudentId, EquipmentId, CheckoutDate, ExpectedReturnDate, Status) " +
                "VALUES ('B5', 'S1', 'E2', '2026-01-01', '2026-01-10', 6);"));
        await Assert.ThrowsAsync<SqliteException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "INSERT INTO Borrowings " +
                "(Id, StudentId, EquipmentId, CheckoutDate, ExpectedReturnDate, Status) " +
                "VALUES ('B6', 'S1', 'E1', '2026-01-01', '2026-01-10', 0);"));
    }
}
