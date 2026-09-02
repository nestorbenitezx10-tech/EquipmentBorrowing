using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowServiceTests
{
    [Fact]
    public async Task RequestBorrowing_ValidInputs_ShouldCreateBorrowingRequest()
    {
        // Arrange
        var studentRepo = new InMemoryStudentRepository();
        var equipmentRepo = new InMemoryEquipmentRepository();
        var borrowingRepo = new InMemoryBorrowingRepository();
        var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

        var student = new Student { Id = "S1", Name = "Test", IsActive = true };
        var equipment = new Equipment { Id = "E1", Name = "Laptop", IsAvailable = true };
        await studentRepo.SaveAsync(student);
        await equipmentRepo.SaveAsync(equipment);

        // Act
        var result = await service.RequestBorrowingAsync("S1", "E1", 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("S1", result.StudentId);
        Assert.Equal("E1", result.EquipmentId);
        Assert.Equal(BorrowingStatus.Requested, result.Status);
    }

    [Fact]
    public async Task RequestBorrowing_UnavailableEquipment_ShouldThrowException()
    {
        // Arrange
        var studentRepo = new InMemoryStudentRepository();
        var equipmentRepo = new InMemoryEquipmentRepository();
        var borrowingRepo = new InMemoryBorrowingRepository();
        var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

        var student = new Student { Id = "S1", Name = "Test", IsActive = true };
        var equipment = new Equipment { Id = "E1", Name = "Laptop", IsAvailable = false };
        await studentRepo.SaveAsync(student);
        await equipmentRepo.SaveAsync(equipment);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.RequestBorrowingAsync("S1", "E1", 5));
    }
}
