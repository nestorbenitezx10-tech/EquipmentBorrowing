using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

Console.WriteLine("=== Equipment Borrowing System Demonstration ===");

// 1. Setup Architecture Dependencies
var studentRepo = new InMemoryStudentRepository();
var equipmentRepo = new InMemoryEquipmentRepository();
var borrowingRepo = new InMemoryBorrowingRepository();
var borrowService = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

// 2. Seed Mock Database Data
var mockStudent = new Student { Id = "STU101", Name = "Alice Smith", Email = "alice@uni.edu" };
var mockEquipment = new Equipment { Id = "EQ-LAP-01", Name = "Dell XPS Laptop", AssetTag = "TAG-00921" };

await studentRepo.SaveAsync(mockStudent);
await equipmentRepo.SaveAsync(mockEquipment);

try
{
    // 3. Process Valid Request Sequence
    Console.WriteLine($"\n[Step 1] Requesting item '{mockEquipment.Name}' for student '{mockStudent.Name}'...");
    var activeBorrow = await borrowService.RequestBorrowingAsync(mockStudent.Id, mockEquipment.Id, 7);
    Console.WriteLine($"-> Success! Request Created with ID: {activeBorrow.Id}. Status: {activeBorrow.Status}");

    // 4. Process Status Lifecycle Progression
    Console.WriteLine($"\n[Step 2] Confirming equipment pick up...");
    await borrowService.ConfirmPickupAsync(activeBorrow.Id);
    
    var updatedEquipment = await equipmentRepo.GetByIdAsync(mockEquipment.Id);
    Console.WriteLine($"-> Success! System State Updated.");
    Console.WriteLine($"-> Borrowing Status: {activeBorrow.Status}");
    Console.WriteLine($"-> Equipment IsAvailable flag: {updatedEquipment?.IsAvailable}");

    // 5. Verify Core Domain Validation Rule (Enforcing Exclusivity)
    Console.WriteLine($"\n[Step 3] Attempting illegal borrow request (Same item while unavailable)...");
    await borrowService.RequestBorrowingAsync(mockStudent.Id, mockEquipment.Id, 5);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"-> Caught expected business rule exception: {ex.Message}");
}

Console.WriteLine("\n=== Demonstration Completed Safely ===");
